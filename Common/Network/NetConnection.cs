using Google.Protobuf;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common.Network
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
        private DataReceivedEventCallback DataReceived;//数据接收完成事件
        private OnDisconnectedEventCallback OnDisconnected;//连接断开事件
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
            len.disconnectedHandler += (Socket soc) => OnDisconnected?.Invoke(this);
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
        public void Send(Package package)
        {
            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            {
                package.WriteTo(ms);
                data = new byte[4 + ms.Length];
                Buffer.BlockCopy(BitConverter.GetBytes((int)ms.Length), 0, data, 0, 4);
                Buffer.BlockCopy(ms.GetBuffer(), 0, data, 4, (int)ms.Length);
            }
            Send(data, 0, data.Length);

        }
        public void Send(byte[] data , int offset, int length)
        {
            if (socket.Connected)
            {
                socket.BeginSend(data, offset, length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            //发送的字节数
            int bytesSent = socket.EndSend(ar);
        }
    }
}
