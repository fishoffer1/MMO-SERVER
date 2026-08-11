using Common.Proto;
using GameServer.Mgr;
using GameServer.Model;
using Serilog;
using Summer;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Service
{
    /// <summary>
    /// 玩家服务
    /// 注册，登录，登出，角色创建，角色删除，角色选择
    /// </summary>
    internal class UserService : Singleton<UserService>
    {
       
        public void Start()
        {
            MessageRouter.Instance.Subscribe<GameEnterRequest>(_GameEnterRequest);
            
        }

        private void _GameEnterRequest(Connection conn, GameEnterRequest msg)
        {

            Log.Information($"收到玩家进入游戏请求，角色ID：{msg.CharacterId}");
            int entityId = EntityManager.Instance.NewEntityId();
            Random random = new Random();
            Vector3Int pos = new Vector3Int(500 + random.Next(-5,5) ,0,500 + random.Next(-5, 5));
            Character character = new Character(entityId, pos ,Vector3Int.zero);
            //通知玩家登录成功
            GameEnterResponse resp = new GameEnterResponse();
            resp.Success = true;
            resp.Entity = character.GetData();
            conn.Send(resp);
            //将新角色加入到地图
            var space = SpaceService.Instance.GetSpace(6);
            space.CharacterJoin(conn,character);
        }
    }
}
