
using GameServer.Network;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Summer.Network;
using Proto;
using Common;
using Serilog;
using GameServer.Service;
using GameServer.Mgr;
using Summer;
using GameServer.Model;
using GameServer.AI;

namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //初始化日志环境
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() //debug , info , warn , error
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File("logs\\server-log.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();


            //Db.fsql.Insert(new DbPlayer()).ExecuteAffrows();
            //Db.fsql.Insert(new DbCharacter()).ExecuteAffrows();

            //加载JSON配置文件
            DataManager.Instance.Init();

            //网路服务模块
            NetService netService = new NetService();
            netService.Start();
            Log.Debug("网络服务启动完成");

            UserService userService = UserService.Instance;
            userService.Start();
            Log.Debug("玩家服务启动完成");

            SpaceService spaceService = SpaceService.Instance;
            spaceService.Start();
            Log.Debug("地图服务启动完成");

            BattleService.Instance.Start();
            Log.Debug("战斗服务启动完成");

            Scheduler.Instance.Start();
            Log.Debug("中心计时器启动完成");

            ChatService.Instance.Start();
            Log.Information("聊天服务启动完成");

            /*Space space = SpaceManager.Instance.GetSpace(2);
            Monster mon = space.MonsterManager.Create(1002, 3, new Vector3Int(270038, 0, 322005), Vector3Int.zero);
            mon.AI = new MonsterAI(mon);*/

            Scheduler.Instance.AddTask(() => {
                EntityManager.Instance.Update();
                SpaceManager.Instance.Update();
            }, 0.02f);

            //消息订阅：用户登录请求
            //MessageRouter.Instance.Subscribe<UserLoginRequest>(OnUserLoginRequest);
            //Log.Debug("用户登录请求的订阅");

            ItemEntity.Create(1, 1001, 10, new Vector3Int(0, 0, 0), Vector3Int.zero);
            ItemEntity.Create(1, 1002, 5, new Vector3Int(3000, 0, 3000), Vector3Int.zero);
            ItemEntity.Create(1, 1003, 1, new Vector3Int(6000, 0, 4000), Vector3Int.zero);

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        
    }
}