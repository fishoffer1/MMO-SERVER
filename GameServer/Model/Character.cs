
using GameServer.Database;
using GameServer.InventorySystem;
using GameServer.Mgr;
using Google.Protobuf;
using Proto;
using Serilog;
using Summer;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
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

        //背包
        public Inventory knapsack;

        //玩家角色唯一ID
        public int characterId => Data.Id;

        public Character(DbCharacter dbChr)
            : base(EntityType.Character, dbChr.JobId, dbChr.Level, new Vector3Int(dbChr.X, dbChr.Y, dbChr.Z), Vector3Int.zero)
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

            var Chr = this;
            //创建背包
            knapsack = new Inventory(this);
            knapsack.Init(Data.Knapsack);
            //创建物品
            /*knapsack.AddItem(1003); //勇士徽章
            knapsack.AddItem(1003);
            knapsack.AddItem(1003);
            knapsack.AddItem(1003);
            knapsack.AddItem(1001); //血瓶
            knapsack.AddItem(1001);
            knapsack.AddItem(1001);
            //删除3个徽章
            knapsack.RemoveItem(1003, 3);*/

            this.Info.KnapsackInfo = knapsack.InventoryInfo;
            Log.Information("Chr[{0}]Knapsack[{1}],Len={2}", Chr.characterId, Chr.Info.KnapsackInfo, Chr.Info.KnapsackInfo.ToByteArray().Length);
        }


    }
}
