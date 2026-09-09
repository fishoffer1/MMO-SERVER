using GameServer.Battle;
using GameServer.Mgr;
using GameServer.Model;
using Proto;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Fight
{
    /// <summary>
    /// 战斗管理器
    /// </summary>
    public class FightMgr
    {
        private Space Space { get; }

        public FightMgr(Space space)
        {
            this.Space = space;
        }

        public FightMgr()
        {

        }

        //投射物列表
        public List<Missile> Missiles = new List<Missile>();

        //技能施法队列
        public ConcurrentQueue<CastInfo> CastQueue = new ConcurrentQueue<CastInfo>();
        //等待广播的队列
        public ConcurrentQueue<CastInfo> SpellQueue = new();
        //等待广播的伤害队列
        public ConcurrentQueue<Damage> DamageQueue = new();
        //角色属性变动
        public ConcurrentQueue<PropertyUpdate> PropertyUpdateQueue = new();

        //施法响应对象，每帧发送一次
        private SpellResponse SpellResponse = new();
        //伤害消息，每帧发送一次
        private DamageResponse DamageResponse = new();
        //角色属性变化
        private PropertyUpdateResponse PropertyUpdateResponse = new();

        public void OnUpdate(float delta)
        {
            while (CastQueue.TryDequeue(out var cast))
            {
                Log.Information("执行施法：{0}", cast);
                RunCast(cast);
            }
            for(int i = 0;i < Missiles.Count; i++)
            {
                Missiles[i].OnUpdate(delta);
            }
            
            BroadcastSpell();
            BroadcastDamage();
            BroadcastProperties();
        }

        private void BroadcastProperties()
        {
            while (PropertyUpdateQueue.TryDequeue(out var item))
            {
                PropertyUpdateResponse.List.Add(item);
            }
            if (PropertyUpdateResponse.List.Count > 0)
            {
                Space.Broadcast(PropertyUpdateResponse);
                PropertyUpdateResponse.List.Clear();
            }
        }

        private void BroadcastDamage()
        {
            while (DamageQueue.TryDequeue(out var item))
            {
                DamageResponse.List.Add(item);
            }
            if (DamageResponse.List.Count > 0)
            {
                Space.Broadcast(DamageResponse);
                DamageResponse.List.Clear();
            }
        }

        //广播施法信息
        private void BroadcastSpell()
        {
            while(SpellQueue.TryDequeue(out var item))
            {
                SpellResponse.CastList.Add(item);
            }
            if(SpellResponse.CastList.Count > 0)
            {
                Space.Broadcast(SpellResponse);
                SpellResponse.CastList.Clear();
            }
        }

        private void RunCast(CastInfo cast)
        {
            var caster = EntityManager.Instance.GetEntity(cast.CasterId) as Actor;
            if (caster == null)
            {
                Log.Error("RunCast: Caster is null {0}", cast.CasterId);
                return;
            }
            caster.Spell.RunCast(cast);
        }
    }
}
