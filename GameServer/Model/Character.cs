using Common.Database;
using Common.Proto;
using GameServer.Mgr;
using Summer;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    public class Character : Actor
    {
        //当前角色的客户端连接
        public Connection conn;
        public Character(int entityId, Vector3Int position, Vector3Int direction) : base(entityId, position, direction)
        {

        }

        public static implicit operator Character(DbCharacter r)
        {
            //申请EntityId
            int entityId = EntityManager.Instance.NewEntityId();
            Character c = new Character(entityId, new Vector3Int(r.X, r.Y, r.Z), Vector3Int.zero);
            c.Id = r.Id;
            c.Name = r.Name;
            c.Info.Id = r.Id;
            c.Info.Name = r.Name;
            c.Info.TypeId = r.JobId;
            c.Info.Level = r.Level;
            c.Info.Exp = r.Exp;
            c.Info.SpaceId = r.SpaceId;
            c.Info.Gold = r.Gold;
            c.Info.Hp = r.Hp;
            c.Info.Mp = r.Mp;
            return c;
        }
    }
}
