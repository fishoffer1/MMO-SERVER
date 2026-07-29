using Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Network
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
        int threadCount = 1;//线程数量
        int WorkerCount = 0;//当前工作线程数量
        bool isRunning = false;//是否正在运行
        /// <summary>
        /// 消息队列，所有的消息都先进入队列，然后由路由器分发到各个模块
        /// </summary>
        private Queue<MsgUnit> messageQueue = new Queue<MsgUnit>();

        /// <summary>
        /// 消息处理函数委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public delegate void MassageHandler<T>(NetConnection sender, Google.Protobuf.IMessage message);
        /// <summary>
        /// 消息频道字典，存储消息类型和对应的处理函数
        /// </summary>
        private Dictionary<string, Delegate> delagateMap = new Dictionary<string, Delegate>();

        //订阅
        public void on<T>(MassageHandler<T> sender) where T : Google.Protobuf.IMessage
        {
            string type = typeof(T).Name;
            if (!delagateMap.ContainsKey(type))
            {
                delagateMap[type] = null;
            }
            delagateMap[type] = (MassageHandler<T>)delagateMap[type] + sender;
            Console.WriteLine(delagateMap[type].GetInvocationList().Length);
        }
        //退订
        public void off<T>(MassageHandler<T> sender) where T : Google.Protobuf.IMessage
        {
            string type = typeof(T).Name;
            if (!delagateMap.ContainsKey(type))
            {
                delagateMap[type] = null;
            }
            delagateMap[type] = (MassageHandler<T>)delagateMap[type] - sender;
        }
        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息内容</param>
        public void AddMessage(NetConnection sender, Google.Protobuf.IMessage message)
        {

            messageQueue.Enqueue(new MsgUnit { sender = sender, message = message });
        }
    
        public void Stop()
        {
            isRunning = false;
            messageQueue.Clear();
            while (messageQueue.Count > 0)
            {
                Thread.Sleep(30);
            }
        }

        public void Start(int ThreadCount)
        {
            isRunning = true;
            this.threadCount = Math.Max(ThreadCount, 1);
            this.threadCount = Math.Min(ThreadCount, 200);
            for(int i = 0; i < this.threadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageWorker))
            }
            while (WorkerCount< this.threadCount)
            {
                Thread.Sleep(100);
            }
        }

        private void MessageWorker(object? state)
        {
            Console.WriteLine("消息处理线程启动");
            try
            {
                Interlocked.Increment(ref this.threadCount);
                //一直工作循环，直到程序退出
                while (isRunning)
                {

                }
            }
            catch
            {

            }
            finally
            {
                Interlocked.Decrement(ref this.threadCount);
            }
            
            
            Console.WriteLine("消息处理线程退出");
        }
    }
}