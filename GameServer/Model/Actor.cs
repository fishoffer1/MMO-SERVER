using Common.Proto;
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
        public int Id { get; set; }
        public string Name { get; set; }
        public Space Space { get; set; }
        public EntityType Type { get; set; }

        public NCharacter Info { get; set; } = new NCharacter();
        public UnitDefine Define { get; set; }
        public Actor(EntityType Type,int TID, Vector3Int position, Vector3Int direction) : base( position, direction)
        {
            this.Type = Type;
            this.Define = DataManager.Instance.Units[TID];
            this.Info.Name = Define.Name;
            this.Info.Tid = TID;
            this.Info.EntityType = Type;
            this.Speed = Define.Speed;
        }
    }
}
