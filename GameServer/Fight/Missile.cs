using GameServer.Battle;
using GameServer.Core;
using GameServer.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Fight
{
    /// <summary>
    /// 投射物
    /// </summary>
    public class Missile
    {
        //所属技能
        public Skill Skill { get; private set; }
        //追击目标
        public SCObject Target { get; private set; }
        //初始位置
        public Vector3 InitPos { get; private set; }
        //飞弹当前位置
        public Vector3 Position;

        public FightMgr FightMgr => Skill.FightMgr;

        public Missile(Skill skill, Vector3 initPos, SCObject target)
        {
            this.Skill = skill;
            this.Target = target;
            this.InitPos = initPos;
            this.Position = initPos;
            Log.Information("Position:{0}", Position);
        }

        public void OnUpdate(float dt)
        {
            var a = this.Position;
            var b = this.Target.Position;
            Vector3 direction = (b - a).normalized;
            var dist = Skill.Def.MissileSpeed * dt;
            if(dist >= Vector3.Distance(a, b))
            {
                Position = b;
                Skill.OnHit(Target);
                FightMgr.Missiles.Remove(this);
            }
            else
            {
                Position = Position + direction * dist;
            }
        }

    }
}
