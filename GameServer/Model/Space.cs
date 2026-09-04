using Common.Proto;
using Serilog;
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

        
        //当前场景中的全部角色<ChrId,ChrObj> 
        private Dictionary<int , Character> CharacterDict = new Dictionary<int, Character>();

        private Dictionary<Connection, Character> ConnCharacter = new Dictionary<Connection, Character>();

        public Space(){ }
        public Space(SpaceDefine def) 
        {
            this.Def = def;
            this.Id = def.SID;
            this.Name = def.Name;
        }
        //角色加入空间
        public void CharacterJoin(Connection conn,Character chr)
        {
            Log.Information("角色进入场景：{0}", chr.entityId);
            conn.Set<Character>(chr);     //把角色存入连接当中
            
            chr.Space = this;

            CharacterDict[chr.Id] = chr;
            chr.conn = conn;
            if (!ConnCharacter.ContainsKey(conn))
            {
                ConnCharacter[conn] = chr;
            }
            //把新进入的角色广播给其他玩家
            var resp = new SpaceCharactersEnterResponse();
            resp.SpaceId = this.Id;
            chr.Info.Entity = chr.EntityData;
            resp.CharacterList.Add(chr.Info);
            foreach (var kv in CharacterDict)
            {
                if(kv.Value.conn != conn)
                {
                    //发送角色进入场景消息
                    kv.Value.conn.Send(resp);
                }
                
            }
            foreach (var kv in CharacterDict)
            {
                if (kv.Value.conn == conn) continue;
                resp.CharacterList.Clear();
                resp.CharacterList.Add(kv.Value.Info);
                conn.Send(resp);
            }
        }
        
        /// <summary>
        /// 角色离开地图
        /// 客户端离线，切换地图
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="chr"></param>
        public void CharacterLeave(Connection conn,Character chr)
        {
            Log.Information("角色离开场景：{0}", chr.Id);
            
            CharacterDict.Remove(chr.Id);
            SpaceCharacterLeaveResponse resp = new SpaceCharacterLeaveResponse();
            resp.EntityId = chr.entityId;
            foreach (var kv in CharacterDict)
            {
                kv.Value.conn.Send(resp);
            }

        }

        /// <summary>
        /// 更新客户端的Entity信息
        /// </summary>
        /// <param name="entitySync"></param>
        public void UpdateEntity(NEntitySync entitySync)
        {
            Log.Information("UpdateEntity{0}" + entitySync);
            foreach (var kv in CharacterDict)
            {
                if(kv.Value.entityId == entitySync.Entity.Id)
                {
                    
                    kv.Value.EntityData = entitySync.Entity;
                    var chr = kv.Value;//自己的角色
                    chr.Data.X = entitySync.Entity.Position.X;
                    chr.Data.Y = entitySync.Entity.Position.Y;
                    chr.Data.Z = entitySync.Entity.Position.Z;
                }
                else
                {
                    SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
                    resp.EntitySync = entitySync;
                    kv.Value.conn.Send(resp);
                }

            }
        }
    }
}
