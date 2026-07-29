using Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Network
{
    class MsgUnit
    {
        public NetConnection sender;
        public Google.Protobuf.IMessage message;
    }
    /// <summary>
    /// 消息转发器
    /// </summary>
    public class MassageRouter : Singleton<MassageRouter>
    {
        /// <summary>
        /// 消息队列，所有的消息都先进入队列，然后由路由器分发到各个模块
        /// </summary>
        private Queue<MsgUnit> messageQueue = new Queue<MsgUnit>();

        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息内容</param>
        public void AddMessage(NetConnection sender, Google.Protobuf.IMessage message)
        {

            messageQueue.Enqueue(new MsgUnit { sender = sender, message = message });
        }
    }
    
        
}
