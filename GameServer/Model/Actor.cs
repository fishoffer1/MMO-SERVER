using Common.Proto;
using GameServer.Battle;
using GameServer.Mgr;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    /// <summary>
    /// 角色基类
    /// </summary>
    public class Actor : Entity
    {
        public int Id { get { return Info.Id; } set{ Info.Id = value; } }
        public string Name { get; set; }
        public Space Space { get; set; }
        public EntityType Type { get { return Info.EntityType; } set { Info.EntityType = value; } }

        public NCharacter Info { get; set; } = new NCharacter();
        public UnitDefine Define { get; set; }
        public EntityState State;
        public Attributes Attr { get; set; } = new Attributes();
        public bool IsDeath; //角色是否死亡
        public Actor(EntityType Type,int TID,int level, Vector3Int position, Vector3Int direction) : base( position, direction)
        {
            this.Define = DataManager.Instance.Units[TID];
            this.Info.Name = Define.Name;
            this.Info.Tid = TID;
            this.Info.EntityType = Type;
            this.Info.Level = level;
            this.Info.Entity = this.EntityData;
            this.Speed = Define.Speed;
        }

        public void OnEnterSpace(Space space)
        {
            this.Space = space;
            this.Info.SpaceId = space.Id;
            //EntityManager.Instance.AddEntity(space.Id, this);
        }

        public void Revive()
        {
            this.IsDeath = false;
        }
    }
}
