
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;
using Google.Protobuf;
using Proto;
using Serilog;
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
            Log.Information("有客户连接" );
      
        }

        private static void OnDataReceived(Connection conn, Google.Protobuf.IMessage data)
        {
            MassageRouter.Instance.AddMessage(conn, data);
        }

        private static void OnDisconnected(Connection conn)
        {
            Log.Information("客户端断开连接");
        }

        
    }
}
