using Common;
using GameServer.Core;
using GameServer.Fight;
using GameServer.Mgr;
using Google.Protobuf;
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
    public class Space
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public SpaceDefine Def { get; set; }
        public FightMgr FightMgr { get; set; }
        
        //当前场景中的全部角色<ChrId,ChrObj> 
        private Dictionary<int , Character> CharacterDict = new Dictionary<int, Character>();
        //当前场景中的全部怪物<MonsterId,ChrObj>
        private Dictionary<int , Monster> MonsterDict = new Dictionary<int, Monster>();

        private Dictionary<Connection, Character> ConnCharacter = new Dictionary<Connection, Character>();


        public MonsterManager MonsterManager = new MonsterManager();
        public SpawnManager SpawnManager = new SpawnManager();
        

        
   
        public Space(SpaceDefine def) 
        {
            this.Def = def;
            this.Id = def.SID;
            this.Name = def.Name;
            this.FightMgr = new FightMgr();
            MonsterManager.Init(this);
            SpawnManager.Init(this);
        }
        //角色加入空间
        public void CharacterJoin(Character chr)
        {
            Log.Information("角色进入场景:" + chr.Id);

            chr.OnEnterSpace(this);

            CharacterDict[chr.Id] = chr;
            if (!ConnCharacter.ContainsKey(chr.conn))
            {
                ConnCharacter[chr.conn] = chr;
            }
            //把新进入的角色广播给其他玩家
            var resp = new SpaceCharactersEnterResponse();
            resp.SpaceId = this.Id; //场景ID
            resp.CharacterList.Add(chr.Info);
            foreach (var kv in CharacterDict)
            {
                if (kv.Value.conn != chr.conn)
                {
                    kv.Value.conn.Send(resp);
                }
            }
            //新上线的角色需要获取全部角色
            SpaceEnterResponse ser = new SpaceEnterResponse();
            ser.Character = chr.Info;
            foreach (var kv in CharacterDict)
            {
                if (kv.Value.conn == chr.conn) continue;
                ser.List.Add(kv.Value.Info);
            }
            foreach (var kv in MonsterDict)
            {
                ser.List.Add(kv.Value.Info);
            }
            chr.conn.Send(ser);
        }

        /// <summary>
        /// 角色离开地图
        /// 客户端离线，切换地图
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="chr"></param>
        public void CharacterLeave(Character chr)
        {
            Log.Information("角色离开场景:" + chr.Id);
            CharacterDict.Remove(chr.Id);
            SpaceCharacterLeaveResponse resp = new SpaceCharacterLeaveResponse();
            resp.EntityId = chr.entityId;
            foreach (var kv in CharacterDict)
            {
                kv.Value.conn.Send(resp);
            }
        }

        /// <summary>
        /// 同场景传送
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        public void Telport(Actor actor, Vector3Int pos, Vector3Int dir = new())
        {
            actor.Position = pos;
            actor.Direction = dir;
            SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
            resp.EntitySync = new NetEntitySync();
            resp.EntitySync.Entity = actor.EntityData;
            resp.EntitySync.Force = true;
            Broadcast(resp);
        }

        /// <summary>
        /// 更新客户端的Entity信息
        /// </summary>
        /// <param name="entitySync"></param>
        public void UpdateEntity(NetEntitySync entitySync)
        {
            Log.Information("UpdateEntity{0}" + entitySync);
            foreach (var kv in CharacterDict)
            {
                if(kv.Value.entityId == entitySync.Entity.Id)
                {
                    
                    kv.Value.EntityData = entitySync.Entity;
                }
                else
                {
                    SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
                    resp.EntitySync = entitySync;
                    kv.Value.conn.Send(resp);
                }

            }
        }

        public void MonsterEnter(Monster mon)
        {
            MonsterDict[mon.Id] = mon;
            mon.OnEnterSpace(this);
            var resp = new SpaceCharactersEnterResponse();
            resp.SpaceId = this.Id;
            resp.CharacterList.Add(mon.Info);
            foreach (var kv in CharacterDict)
            {
                kv.Value.conn.Send(resp);
            }
        }

        /// <summary>
        /// 广播Proto消息给场景的全体玩家
        /// </summary>
        /// <param name="msg"></param>
        public void Broadcast(IMessage msg)
        {
            foreach (var kv in CharacterDict)
            {
                kv.Value.conn.Send(msg);
            }
        }

        public void Update()
        {
            this.SpawnManager.Update();
            this.FightMgr.OnUpdate(Time.deltaTime);
        }
    }
}
