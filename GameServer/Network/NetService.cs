
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Summer;
using Summer.Network;

namespace GameServer.Network
{/// <summary>
 /// 网络服务类，负责初始化和启动网络监听
 /// <summary>
    public class NetService
    {
        TcpServer TcpServer;

        public NetService()
        {
            TcpServer = new TcpServer("0.0.0.0", 32510);
            TcpServer.Connected += OnClientCoenneted;
            TcpServer.DataReceived += OnDataReceived;
            TcpServer.Disconnected += OnDisconnected;
        }
        public void Start()
        {
            //启动网络监听
            TcpServer.Start();
            //启动消息分发器
            MassageRouter.Instance.Start(10);
        }



        static void OnClientCoenneted(Connection conn)
        {
            //当有客户端连入时触发
            Console.WriteLine("有客户连接" );
      
        }

        private static void OnDataReceived(Connection conn, byte[] data)
        {
            //Console.WriteLine("收到客户端数据，长度：" + data.Length);
            Package package = Package.Parser.ParseFrom(data);
           
            MassageRouter.Instance.AddMessage(conn, package);
        }

        private static void OnDisconnected(Connection conn)
        {
            Console.WriteLine("客户端断开连接");
        }

        
    }
}
