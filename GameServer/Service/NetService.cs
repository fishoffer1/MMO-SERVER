using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Summer;
using Common;
using Serilog;

namespace GameServer.Network
{
    /// <summary>
    /// 网络服务
    /// </summary>
    public class NetService
    {

        TcpServer tcpServer;

        public NetService()
        {
            tcpServer = new TcpServer("0.0.0.0", 32510);
            tcpServer.Connected += OnClientConnected;
            tcpServer.Disconnected += OnDisconnected;
        }


        public void Start() {
            //启动网络监听，指定消息包装类型
            tcpServer.Start();
            //启动消息分发器
            MessageRouter.Instance.Start(10);
        }


        // 当客户端接入
        private void OnClientConnected(Connection conn)
        {
            Log.Information("客户端接入");
            conn.Set<string>("嘻嘻哈哈");
            //
        }

        private void OnDisconnected(Connection conn)
        {
            Log.Information("连接断开:"+conn);
        }

    }
}
