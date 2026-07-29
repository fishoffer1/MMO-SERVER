using Common.Network;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Network
{/// <summary>
 /// 网络服务类，负责初始化和启动网络监听
 /// <summary>
    public class NetService
    {
        //网络监听器
        TcpSocketListener listener = null;
        public void Init(int port)
        {
            Console.WriteLine("Hello, World!");
            listener = new TcpSocketListener("0.0.0.0", port);
            listener.SocketConnected += OnClientCoenneted;
            
           // Console.ReadKey();
            // Initialization code for the network service
        }

        public void Start()
        {
            listener.Start();
        }
         static void OnClientCoenneted(object? sender, Socket Socket)
        {
            var ipe = Socket.RemoteEndPoint as IPEndPoint;//向下转型,类型还原；
            Console.WriteLine("有客户连接" + ipe.Address);
            //当有客户端连入时触发
            new NetConnection(Socket,
                new NetConnection.DataReceivedEventCallback(OnDataReceived), 
                new NetConnection.OnDisconnectedEventCallback(OnDisconnected));
            
        }

        private static void OnDataReceived(NetConnection shader, byte[] data)
        {
            User user = User.Parser.ParseFrom(data);
            //Vector3 vector = Vector3.Parser.ParseFrom(data);
            String str = Encoding.UTF8.GetString(data);
            Console.WriteLine(str);
            Console.WriteLine("收到客户端消息: " + user.Id + " " + user.Name);
            MassageRouter.Instance.AddMessage(shader, user);
        }

        private static void OnDisconnected(NetConnection shader)
        {
            Console.WriteLine("客户端断开连接");
        }

        
    }
}
