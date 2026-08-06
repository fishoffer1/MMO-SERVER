
using Common;
using Common.Proto;
using GameServer.Network;
using Google.Protobuf;
using Serilog;
using Summer;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // 设置日志的最小级别为 Debug
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/server-log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            NetService netService = new NetService();
            
            netService.Start();

            MassageRouter.Instance.on<UserLoginRequest>(OnUserLoginRequest);    
                
            while(true){
                Thread.Sleep(100);
            }
        }

        private static void OnUserLoginRequest(Connection sender, UserLoginRequest message)
        {
            Log.Information("发现用户登录请求：{0} {1}", message.Username, message.Password);
        }

       
    }
}
