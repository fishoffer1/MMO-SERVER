using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Network
{
    /// <summary>
    /// 客户端网络连接
    /// 职责：发送消息，接收消息，关闭连接，断开通知。
    /// </summary>
    public class NetConnection
    {
        public delegate void DataReceivedEventCallback(NetConnection shader, byte[] data);
        public delegate void OnDisconnectedEventCallback(NetConnection shader);
       
        public Socket socket;
        private DataReceivedEventCallback DataReceived;
        private OnDisconnectedEventCallback OnDisconnected;
        /// <summary>
        /// 关闭连接
        /// </summary>
        public NetConnection(Socket socket , DataReceivedEventCallback cb1, OnDisconnectedEventCallback cb2)
        {
            this.socket = socket;
            this.DataReceived = cb1;
            this.OnDisconnected = cb2;

            var len = new LengthFieldDecoder(socket, 64 * 1024, 0, 4, 0, 4);
            len.DataReceived += Len_DataReceived;
            len.disconnectedHandler += (Socket soc) => OnDisconnected(this);
            len.Start();
        }

        private void Ondisconnected(Socket soc)
        {
            OnDisconnected(this);
        }

        private void Len_DataReceived(byte[] buffer)
        {
            DataReceived?.Invoke(this, buffer);
        }
        public void Close()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Console.WriteLine("关闭连接异常：" + ex.Message);
            }

            socket.Close();
            socket = null;
            OnDisconnected?.Invoke(this);
        }
    }
}
