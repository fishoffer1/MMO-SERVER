using Common.Database;
using Common.Proto;
using GameServer.Mgr;
using Serilog;
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
        //当前角色的数据库数据对象
        public DbCharacter Data;
        public Character(Vector3Int position, Vector3Int direction) : base(EntityType.Character, 0,0, position, direction)
        {

        }

        public Character(DbCharacter dbChr):base(EntityType.Character, dbChr.JobId,dbChr.Level, new Vector3Int(dbChr.X, dbChr.Y, dbChr.Z),Vector3Int.zero)
        {
            UnitDefine ud = DataManager.Instance.Units[dbChr.JobId];
            this.Id = dbChr.Id;
            this.Name = dbChr.Name;
            this.Info.Id = dbChr.Id;
            this.Info.Name = dbChr.Name;
            this.Info.Tid = dbChr.JobId;
            this.Info.Exp = dbChr.Exp;
            this.Info.SpaceId = dbChr.SpaceId;
            this.Info.Gold = dbChr.Gold;
            this.Info.Hp = dbChr.Hp;
            this.Info.Mp = dbChr.Mp;
            this.Data = dbChr;
            this.Speed = ud.Speed;
        }
        
        public static implicit operator Character(DbCharacter dbChr)
        {
           
            return new Character(dbChr);
        }
    }
}
