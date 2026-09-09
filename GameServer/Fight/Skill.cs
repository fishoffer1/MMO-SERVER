using GameServer.Core;
using GameServer.Fight;
using GameServer.Model;
using Proto;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GameServer.Battle;

public class Skill
{
    //技能阶段
    public enum Stage
    {
        None,       //无状态
        Intonate,   //吟唱
        Active,     //已激活
        Coolding,   //冷却中
    }

    public bool IsPassive;          // 是否被动技能
    public SkillDefine Def;         // 技能设定
    public Actor Owner;             // 技能归属者
    public float Cd;                // 冷却计时，0代表技能可用
    private float _time;            // 技能运行时间
    public Stage State;             // 当前技能状态

    // 强制触发暴击的次数
    int forceCritAfter => (int)(100f / Owner.Attr.Final.CRI) + 2;

    // 未暴击次数
    int notCrit = 0;

    public SCObject Target { get; private set; }

    public bool IsUnitTarget { get => Def.TargetType == "单位"; }
    public bool IsPointTarget { get => Def.TargetType == "点"; }
    public bool IsNoneTarget { get => Def.TargetType == "None"; }
    public bool IsNormal => Def.Type == "普通攻击";

    public FightMgr FightMgr => Owner.Space.FightMgr;



    public Skill(Actor owner, int skid)
    {
        Owner = owner;
        Def = DataManager.Instance.Skills[skid];
        //伤害延迟默认float[]={0}
        if (Def.HitDelay.Length == 0)
        {
            Array.Resize(ref Def.HitDelay, 1);
        }
    }

    public void Update()
    {

        if (State == Stage.None && Cd == 0) return;

        if(Cd > 0) Cd -= Time.deltaTime;
        if(Cd < 0) Cd = 0;
        if (_time < 0.0001f)
        {
            OnIntonate();
        }
        _time += Time.deltaTime;

        if (State== Stage.Intonate && _time >= Def.IntonateTime)
        {
            State = Stage.Active;
            Cd = Def.CD;
            OnActive();
        }

        if(State == Stage.Active )
        {
            if (_time >= Def.IntonateTime + Def.HitDelay.Max())
            {
                State = Stage.Coolding;
            }
        }

        if (State == Stage.Coolding)
        {
            if (Cd==0)
            {
                _time = 0;
                State = Stage.None;
                OnFinish();
            }
        }

    }

    private void OnIntonate()
    {
        Log.Information("技能蓄力：Owner[{0}],Skill[{1}]", Owner.entityId, Def.Name);
    }

    public void OnActive()
    {
        Log.Information("技能激活：Owner[{0}],Skill[{1}]",Owner.entityId,Def.Name);
        //如果是投射物
        if (Def.IsMissile)
        {
            var missile = new Missile(this, Owner.Position, Target);
            FightMgr.Missiles.Add(missile);
        }
        //如果不是投射物
        else
        {
            Log.Information("Def.HitDelay.Length=" + Def.HitDelay.Length);
            for(int i=0;i<Def.HitDelay.Length;i++)
            {
                Scheduler.Instance.AddTask(_hitTrigger, Def.HitDelay[i], 1);
            }
        }
    }

    //触发延迟伤害
    private void _hitTrigger()
    {
        Log.Information("_hitTrigger：Owner[{0}],Skill[{1}]", Owner.entityId, Def.Name);
        OnHit(Target);
    }

    //技能打到目标
    public void OnHit(SCObject sco)
    {
        Log.Information("OnHit：Owner[{0}],Skill[{1}]，SCO[{2}]", Owner.entityId, Def.Name,sco);
        //单体伤害
        if(Def.Area == 0)
        {
            if(sco is SCEntity scEntity)
            {
                var actor = scEntity.RealObj as Actor;
                TakeDamage(actor);
            }
        }
        //范围伤害
        else
        {
            Log.Information("范围伤害：Space[{0}],Center[{1}],Area[{2}]", Owner.Space.Id, sco.Position, Def.Area);
            var list = Game.RangeUnit(Owner.Space.Id, sco.Position, Def.Area);
            foreach (var item in list)
            {
                TakeDamage(item);
            }
        }
    }

    Random rand = new Random();

    //对目标造成伤害
    private void TakeDamage(Actor target)
    {
        if (target.IsDeath || target == Owner) return;
        Log.Information("Skill:TakeDamage:Atker[{0}],Target[{1}]", Owner.entityId, target.entityId);
        //计算伤害数值、暴击、闪避、
        //扣除目标HP、广播通知
        
        //伤害=攻击[攻]×(1-护甲[守]/(护甲[守]+400+85×等级[攻]))

        var a = Owner.Attr.Final;   //攻击者属性
        var b = target.Attr.Final;  //被攻击者属性

        //伤害信息
        Damage dmg = new Damage();
        dmg.AttackerId = Owner.entityId;
        dmg.TargetId = target.entityId;
        dmg.SkillId = Def.ID;
        //技能的物攻和法攻
        var ad = Def.AD + a.AD * Def.ADC;
        var ap = Def.AP + a.AP * Def.APC;
        //计算伤害
        var ads = ad * (1 - b.DEF / (b.DEF + 400 + 85 * Owner.Info.Level));
        var aps = ap * (1 - b.MDEF / (b.MDEF + 400 + 85 * Owner.Info.Level));
        Log.Information("ads={0} , aps={1}", ads, aps);
        dmg.Amount = ads + aps;
        //计算暴击
        notCrit++;
        float randCri = rand.NextSingle();
        float cri = a.CRI * 0.01f;
        Log.Information("暴击计算：{0} / {1} | [{2}/{3}]", randCri, cri, notCrit, forceCritAfter);
        if (randCri < cri || notCrit > forceCritAfter)
        {
            notCrit = 0;
            dmg.IsCrit = true;
            dmg.Amount *= MathF.Max(a.CRD,100) * 0.01f;
        }
        //计算闪避
        var hitRate = (a.HitRate - b.DodgeRate) * 0.01f;
        Log.Information("Hit Rate: {0}", hitRate);
        if (rand.NextDouble() > hitRate)
        {
            dmg.IsMiss = true;
            dmg.Amount = 0;
        }
        //造成伤害
        target.RecvDamage(dmg);

    }





    public void OnFinish()
    {
        Log.Information("技能结束：Owner[{0}],Skill[{1}]]", Owner.entityId, Def.Name);
    }


    //检查技能是否可用
    public CastResult CanUse(SCObject sco)
    {
        //被动技能
        if (IsPassive) 
            return CastResult.IsPassive;

        //MP不足
        else if (Owner.Info.Mp < Def.Cost)
            return CastResult.MpLack;

        //正在进行
        else if (State != Stage.None)
            return CastResult.Running;

        //冷却中
        else if (Cd > 0)
            return CastResult.Cooldown;

        //Entity已经死亡
        else if (Owner.IsDeath)
            return CastResult.EntityDead;

        //目标已经死亡
        else if(sco is SCEntity && (sco.RealObj as Actor).IsDeath)
            return CastResult.EntityDead;

        //施法者和目标的距离
        Log.Information("而这位置：{0} , {1}", Owner.Position, sco.Position);
        var dist = Vector3Int.Distance(Owner.Position,sco.Position);
        Log.Information("施法者和目标的距离：{0} , {1}", dist, Def.SpellRange);
        if (dist > Def.SpellRange)
        {
            return CastResult.OutOfRange;
        }

        return CastResult.Success;
    }

    //使用技能
    public CastResult Use(SCObject sco)
    {
        Target = sco;
        _time = 0;
        State = Stage.Intonate;
        return CastResult.Success;
    }
}
