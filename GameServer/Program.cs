using Common.Network;
using GameServer.Network;
using Google.Protobuf;
using Network;
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

            MassageRouter.Instance.Start(4);//启动消息路由器，使用4个线程处理消息

            MassageRouter.Instance.on<Package>(OnMsgTest);
            Console.ReadKey();
            
        }

        private static void OnMsgTest<Package>(NetConnection sender, Package message)
        {
            
        }
    }
}
