using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
namespace Common.Network
{
    class MsgUnit
    {
        public NetConnection sender;
        public Package message;
    }
    /// <summary>
    /// 消息转发器
    /// </summary>
    public class MassageRouter : Singleton<MassageRouter>
    {
        int threadCount = 1;//线程数量
        int WorkerCount = 0;//当前工作线程数量
        bool isRunning = false;//是否正在运行
        AutoResetEvent threadEvent = new AutoResetEvent(false);//消息事件，用于通知有新消息到来
        /// <summary>
        /// 消息队列，所有的消息都先进入队列，然后由路由器分发到各个模块
        /// </summary>
        private Queue<MsgUnit> messageQueue = new Queue<MsgUnit>();//通过set()唤醒线程处理消息

        /// <summary>
        /// 消息处理函数委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public delegate void MassageHandler<T>(NetConnection sender, T message);
        /// <summary>
        /// 消息频道字典，存储消息类型和对应的处理函数（订阅列表）
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

        //触发
        void Fire<T>(NetConnection sender ,T msg)
        {
            string type = typeof(T).Name;
            if (delagateMap.ContainsKey(type))//是否有订阅者
            {
                MassageHandler<T> handlers = (MassageHandler<T>)delagateMap[type];
               
                
                    try
                    {
                        handlers.Invoke(sender, msg);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Message.Fire error: " + ex.StackTrace);
                       
                    }
                
            }
        }
        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息内容</param>
        public void AddMessage(NetConnection sender, Package message)
        {

            messageQueue.Enqueue(new MsgUnit { sender = sender, message = message });
            threadEvent.Set();//唤醒线程处理消息
        }
    
        public void Stop()
        {
            isRunning = false;
            messageQueue.Clear();
            while (messageQueue.Count > 0)
            {
                threadEvent.Set();
            }
            Thread.Sleep(100);
        }

        public void Start(int ThreadCount)
        {
            isRunning = true;
           
            this.threadCount = Math.Min(Math.Max(ThreadCount, 1), 200);
            for(int i = 0; i < this.threadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageWorker));
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
                Interlocked.Increment(ref this.WorkerCount);
                //一直工作循环，直到程序退出
                while (isRunning)
                {
                    if (messageQueue.Count == 0)
                    {
                        threadEvent.WaitOne();//set()唤醒
                        continue;
                    }
                    //MsgUnit pack = messageQueue.Dequeue();
                    // 从消息队列取出一个元素
                    MsgUnit msgUnit = messageQueue.Dequeue();
                    Package package = msgUnit.message;
                    if (package != null)
                    {
                        if (package.Request != null)
                        {
                            doRequest(msgUnit.sender, package.Request);

                        }
                        if (package.Response != null)
                        {
                            doResponse(msgUnit.sender, package.Response);
                        }
                    }
                }
            }
            catch
            {

            }
            finally
            {
                Interlocked.Decrement(ref this.WorkerCount);
            }
            
            
            Console.WriteLine("消息处理线程退出");
        }

        public void doRequest(NetConnection sender, Request request)
        {
            if (request.UserRegister != null)
            {
                Fire(sender, request.UserRegister);
            }
            if (request.UserLogin != null)
            {
                Fire(sender, request.UserLogin);
            }
        }
        public void doResponse(NetConnection sender, Response response)
        {
            if (response.UserRegister != null)
            {
                Fire(sender, response.UserRegister);
            }
            if (response.UserLogin != null)
            {
               Fire(sender, response.UserLogin);   
            }
        }
    }
}