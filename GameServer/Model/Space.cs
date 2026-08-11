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

        

        private Dictionary<int , Character> CharacterDict = new Dictionary<int, Character>();

        private Dictionary<Connection, Character> ConnCharacter = new Dictionary<Connection, Character>();
        //角色加入空间
        public void CharacterJoin(Connection conn,Character character)
        {
            Log.Information("角色进入场景：{0}", character.entityId);
            Log.Information("获取数值{0}",conn.Get<string>());
            CharacterDict[character.entityId] = character;
            character.conn = conn;
            if (!ConnCharacter.ContainsKey(conn))
            {
                ConnCharacter[conn] = character;
            }
            //把新进入的角色广播给其他玩家
            var resp = new SpaceCharactersEnterResponse();
            resp.SpaceId = this.Id;
            resp.EntityList.Add(character.GetData());
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
                resp.EntityList.Clear();
                resp.EntityList.Add(kv.Value.GetData());
                conn.Send(resp);
            }
        }
        public bool HasConnection(Connection conn)
        {
            return ConnCharacter.ContainsKey((Connection)conn);
        }

        /// <summary>
        /// 更新客户端的Entity信息
        /// </summary>
        /// <param name="entitySync"></param>
        public void UpdateEntity(NEntitySync entitySync)
        {
            Log.Information("UpdateEntity{0}" + entitySync);
        }
    }
}
