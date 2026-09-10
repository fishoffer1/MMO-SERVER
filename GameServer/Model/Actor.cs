using Common.Summer.Core;
using GameServer.Battle;
using GameServer.Fight;
using GameServer.Mgr;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.X509;
using Proto;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{

    
    /// <summary>
    /// 角色基类｛主角、怪物、NPC、陷阱｝
    /// </summary>
    public class Actor : Entity
    {
        public int Id { get { return Info.Id; } set { Info.Id = value; } }
        public string Name { get { return Info.Name; } set { Info.Name = value; } }
        public Space Space { get; set; }
        public EntityType Type { get { return Info.Type; } set { Info.Type = value; } }
        public NetActor Info { get; set; } = new NetActor();
        public UnitDefine Define { get; set; }
        public EntityState State;
        public AttributesAssembly Attr { get; set; } = new AttributesAssembly();

        public UnitState UnitState; //单位状态
        public bool IsDeath => UnitState==UnitState.Dead; //角色是否死亡
        

        public SkillManager skillMgr;

        public Spell Spell;

        public float Hp
        {
            get => Info.Hp;
            set => Info.Hp = value;
        }
        public float Mp
        {
            get => Info.Mp;
            set => Info.Mp = value;
        }


        public Actor(EntityType type, int tid, int level, Vector3Int position, Vector3Int direction) 
            : base(position, direction)
        {
            
            this.Info.Tid = tid;
            this.Info.Type = type;
            this.Info.Level = level;
            this.Info.Entity = this.EntityData;

            if (type != EntityType.Item)
            {
                this.Define = DataManager.Instance.Units[tid];
                this.Info.Name = Define.Name;
                this.Info.Hp = (int)Define.HPMax;
                this.Info.Mp = (int)Define.MPMax;
                this.Speed = Define.Speed;

                this.skillMgr = new(this);
                this.Attr.Init(this);
                this.Spell = new Spell(this);
            }
        }

        public void OnEnterSpace(Space _space)
        {
            if (Space != null && _space != null)
            {
                EntityManager.Instance.ChangeSpace(this, Space.Id, _space.Id);
            }
            this.Space = _space;
            this.Info.SpaceId = _space.Id;
            if(this is Character chr)
            {
                chr.Data.SpaceId = _space.Id;
            }
        }

        public virtual void Revive()
        {
            Log.Information("Actor.Revive:{0}", entityId);
            if (!IsDeath) return;
            SetHp(Attr.Final.HPMax);
            SetMp(Attr.Final.MPMax);
            SetState(UnitState.Free);
        }

        public virtual void TelportSpace(Space _space, Vector3Int pos, Vector3Int dir=new())
        {
            if (this is not Character chr) return;
            if (_space != Space)
            {
                //1.退出当前场景
                Space.EntityLeave(chr);
                //2.设置坐标和方向
                chr.Position = pos;
                chr.Direction = dir;
                //3.进入新场景
                _space.EntityEnter(chr);
            }
            else
            {
                _space.Telport(chr, pos, dir);
            }
            
            
        }

        public override void Update()
        {
            this.skillMgr?.Update();
        }

        //杀死此单位
        public virtual void Die(int killerID)
        {
            if (IsDeath) return;
            OnBeforeDie(killerID);
            SetHp(0);
            SetMp(0);
            SetState(UnitState.Dead);
            OnAfterDie(killerID);
        }
        protected virtual void OnBeforeDie(int killerID)
        {

        }
        protected virtual void OnAfterDie(int killerID)
        {
            
        }

        public void RecvDamage(Damage dmg)
        {
            Log.Information("Actor:RecvDamage[{0}]", dmg);
            //添加广播
            Space.FightMgr.DamageQueue.Enqueue(dmg);
            //扣血或者死亡
            if (Hp > dmg.Amount)
            {
                SetHp(Hp - dmg.Amount);
            }
            else
            {
                Die(dmg.AttackerId);
            }
        }

        private void SetHp(float hp)
        {
            if (MathC.Equals(Info.Hp, hp)) return;
            if (hp <= 0)
            {
                hp = 0;
            }
            if (hp > Attr.Final.HPMax)
            {
                hp = Attr.Final.HPMax;
            }
            float oldValue = Info.Hp;
            Info.Hp = hp;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Prop.Hp,
                OldValue = new() { FloatValue = oldValue },
                NewValue = new() { FloatValue = Info.Hp },
            };
            Space.FightMgr.PropertyUpdateQueue.Enqueue(po);
        }

        private void SetMp(float mp)
        {
            if (MathC.Equals(Info.Mp, mp)) return;
            if (mp <= 0)
            {
                mp = 0;
            }
            if (mp > Attr.Final.MPMax)
            {
                mp = Attr.Final.MPMax;
            }
            float oldValue = Info.Mp;
            Info.Mp = mp;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Prop.Mp,
                OldValue = new() { FloatValue = oldValue },
                NewValue = new() { FloatValue = Info.Mp },
            };
            Space.FightMgr.PropertyUpdateQueue.Enqueue(po);
        }

        private void SetState(UnitState unitState)
        {
            if(this.UnitState == unitState) return;
            UnitState oldValue = this.UnitState;
            this.UnitState = unitState;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Prop.State,
                OldValue = new() { StateValue = oldValue },
                NewValue = new() { StateValue = unitState },
            };
            Space.FightMgr.PropertyUpdateQueue.Enqueue(po);
        }

    }
}
