using Common;
using Google.Protobuf;
using Proto;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Summer
{ 
    /// <summary>
    /// 通用网络连接,可以继承此类实现功能拓展
    /// 职责：发送消息，接收消息，关闭连接，断开通知。
    /// </summary>
    public class Connection
    {
        public delegate void DataReceivedEventCallback(Connection shader, IMessage data);
        public delegate void OnDisconnectedEventCallback(Connection shader);
       
        private Socket socket;
        public Socket Socket { get { return socket; } }
        public DataReceivedEventCallback OnDataReceived;//数据接收完成事件
        public OnDisconnectedEventCallback OnDisconnected;//连接断开事件
        /// <summary>
        /// 关闭连接
        /// </summary>
        public Connection(Socket socket)
        {
            this.socket = socket;
            

            var len = new LengthFieldDecoder(socket, 64 * 1024, 0, 4, 0, 4);
            len.DataReceived += Len_DataReceived;
            len.Disconnected += (Socket soc) => OnDisconnected?.Invoke(this);
            len.Start();
        }

        
        private void Len_DataReceived(byte[] buffer)
        {
            Package package = Package.Parser.ParseFrom(buffer);
            var message = ProtoHelper.Unpack(package);
            OnDataReceived?.Invoke(this, message);
        }
        public void Close()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Log.Error("关闭连接异常：" + ex.Message);
            }

            socket.Close();
            socket = null;
            OnDisconnected?.Invoke(this);
        }
        #region 发送网路数据包的封装
        
        
        public void Send(Google.Protobuf.IMessage message)
        {
            Package pack = ProtoHelper.Pack(message);

            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            {
                pack.WriteTo(ms);
                //对消息进行编码
                data = new byte[4 + ms.Length];
                byte[] lenBytes = BitConverter.GetBytes((int)ms.Length);
                //如果是小端字节序，则需要反转字节数组  
                if (BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
                //拼装数据结果
                Buffer.BlockCopy(lenBytes, 0, data, 0, 4);
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
    #endregion