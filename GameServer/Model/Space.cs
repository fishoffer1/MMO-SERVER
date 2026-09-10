using Serilog;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using System.Runtime.ConstrainedExecution;
using GameServer.Mgr;
using GameServer.Core;
using GameServer.Fight;
using Summer;
using Google.Protobuf;

namespace GameServer.Model
{
    //场景
    public class Space
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SpaceDefine Def { get; set; }

        public FightMgr FightMgr { get; set; }

        //当前场景中全部的主角 <ChrId,ChrObj>
        private Dictionary<int, Character> CharacterDict = new Dictionary<int, Character>();
        //当前场景中的全部演员 <entityId,Actor>
        private Dictionary<int, Actor> ActorDict = new ();

        public MonsterManager MonsterManager = new MonsterManager();
        public SpawnManager SpawnManager = new SpawnManager();


        public Space(SpaceDefine def) {
            this.Def = def;
            this.Id = def.SID;
            this.Name = def.Name;
            this.FightMgr = new FightMgr(this);
            MonsterManager.Init(this);
            SpawnManager.Init(this);
        }

        /// <summary>
        /// 演员进入场景
        /// </summary>
        /// <param name="actor"></param>
        public void EntityEnter(Actor actor)
        {
            Log.Information("角色进入场景:eid=" + actor.entityId);
            ActorDict[actor.entityId] = actor;
            actor.OnEnterSpace(this);
            var resp = new SpaceCharactersEnterResponse();
            resp.SpaceId = this.Id; //场景ID
            resp.CharacterList.Add(actor.Info);
            Broadcast(resp);
            //如果是主角
            if(actor is Character chr)
            {
                CharacterJoin(chr);
            }
        }


        /// <summary>
        /// 演员离开场景
        /// </summary>
        /// <param name="actor"></param>
        public void EntityLeave(Actor actor)
        {
            Log.Information("角色离开场景:" + actor.entityId);
            ActorDict.Remove(actor.entityId);
            SpaceCharacterLeaveResponse resp = new SpaceCharacterLeaveResponse();
            resp.EntityId = actor.entityId;
            Broadcast(resp);
            //如果是主角
            if (actor is Character chr)
            {
                CharacterDict.Remove(chr.entityId);
            }
        }


        //主角进入场景
        private void CharacterJoin(Character chr)
        {
            Log.Information("玩家进入场景 Chr[{0}],Entity[{1}]", chr.characterId, chr.entityId);
            //记录到主角字典
            CharacterDict[chr.entityId] = chr;
            //拉取附近的演员信息
            SpaceEnterResponse ser = new SpaceEnterResponse();
            ser.Character = chr.Info;
            foreach (var kv in ActorDict)
            {
                ser.List.Add(kv.Value.Info);
            }
            chr.conn.Send(ser);
            
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
        /// 广播更新Entity信息
        /// </summary>
        /// <param name="entitySync"></param>
        public void UpdateEntity(NetEntitySync entitySync)
        {
            //Log.Information("UpdateEntity {0}", entitySync);
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
