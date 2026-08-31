using Common.Database;
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
            MessageRouter.Instance.Subscribe<UserLoginRequest>(_UserLoginRequest);
            
        }

        private void _UserLoginRequest(Connection conn, UserLoginRequest msg)
        {
           var dbPlayer= Db.fsql.Select<DbPlayer>()
                            .Where(p => p.UserName == msg.Username)
                            .Where(p => p.Password == msg.Password)
                            .First();
            Log.Information("登录结果：" + dbPlayer);
            UserLoginResponse resp = new UserLoginResponse();
            if(dbPlayer != null)
            {
                
                resp.Success = true; // 登录成功
                resp.Message = "登录成功";
                conn.Set<DbPlayer>(dbPlayer); //登录成功，在conn里记录用户信息
            }
            else
            {
                resp.Success = false;
                resp.Message = "用户名或密码不正确";
            }
            conn.Send(resp);
        }

        private void _GameEnterRequest(Connection conn, GameEnterRequest msg)
        {

            Log.Information($"收到玩家进入游戏请求，角色ID：{msg.CharacterId}");
            int entityId = EntityManager.Instance.NewEntityId();
            Random random = new Random();
            Vector3Int pos = new Vector3Int(500 + random.Next(-5,5) ,0,500 + random.Next(-5, 5));
            pos *= 1000;
            Character character = new Character(entityId, pos ,Vector3Int.zero);
            //通知玩家登录成功
            GameEnterResponse resp = new GameEnterResponse();
            resp.Success = true;
            resp.Entity = character.GetData();
            conn.Send(resp);
            //将新角色加入到地图
            var space = SpaceService.Instance.GetSpace(3);
            space.CharacterJoin(conn,character);
        }
    }
}
