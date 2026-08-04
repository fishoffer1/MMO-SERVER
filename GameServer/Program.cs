
using GameServer.Network;
using Google.Protobuf;
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
            NetService netService = new NetService();
            netService.Init(32510);//初始化网络服务，监听端口32510
            netService.Start();
            MassageRouter.Instance.Start(8);
            //启动消息路由器，使用4个线程处理消息
            //消息订阅
            MassageRouter.Instance.on<UserLoginRequest>(OnUserLoginRequest);    
                
            while(true){
                Thread.Sleep(100);
            }
        }

        private static void OnUserLoginRequest(Connection sender, UserLoginRequest message)
        {
            Console.WriteLine("发现用户登录请求：{0} {1}", message.Username, message.Password);
        }

       
    }
}
