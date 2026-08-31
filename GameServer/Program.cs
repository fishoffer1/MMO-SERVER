
using GameServer.Network;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Summer.Network;

using Common;
using Serilog;
using Common.Proto;
using GameServer.Service;
using Common.Database;



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
                .WriteTo.Async( a => a.File("logs\\server-log.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();

            //Db.fsql.Insert(new DbPlayer()).ExecuteAffrows();
            //Db.fsql.Insert(new DbCharacter()).ExecuteAffrows();

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

            //消息订阅：用户登录请求
            //MessageRouter.Instance.Subscribe<UserLoginRequest>(OnUserLoginRequest);
            //Log.Debug("用户登录请求的订阅");

            while (true)
            {
                Thread.Sleep(16);
            }
        }

        //当消息分发器发现了UserLoginRequest类型数据，就会回调该方法
        private static void OnUserLoginRequest(Connection sender, UserLoginRequest msg)
        {
            //Log.Information("发现用户登录请求：{0} , {1}", msg.Username, msg.Password);
        }
    }
}