using Common.Database;
using GameServer.Mgr;
using Proto;
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
    //角色
    public class Character : Actor
    {
        //当前角色的客户端连接
        public Connection conn;
        //当前角色对应的数据库对象
        public DbCharacter Data;
        //玩家角色唯一ID
        public int characterId => Data.Id;

        public Character(DbCharacter dbChr)
            :base(EntityType.Character,dbChr.JobId, dbChr.Level, new Vector3Int(dbChr.X, dbChr.Y, dbChr.Z), Vector3Int.zero)
        {
            this.Id = dbChr.Id;
            this.Name = dbChr.Name;
            this.Info.Id = dbChr.Id;
            this.Info.Name = dbChr.Name;
            this.Info.Tid = dbChr.JobId; //单位类型
            this.Info.Level = dbChr.Level;
            this.Info.Exp = dbChr.Exp;
            this.Info.SpaceId = dbChr.SpaceId;
            this.Info.Gold = dbChr.Gold;
            this.Info.Hp = dbChr.Hp;
            this.Info.Mp = dbChr.Mp;
            this.Data = dbChr;
            
        }


    }
}
