using Summer;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Summer
{
    class MsgUnit
    {
        public Connection sender;
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
        public delegate void MassageHandler<T>(Connection sender, T message);
        /// <summary>
        /// 消息频道字典，存储消息类型和对应的处理函数（订阅列表）
        /// </summary>
        private Dictionary<string, Delegate> delagateMap = new Dictionary<string, Delegate>();

        //订阅
        public void on<T>(MassageHandler<T> sender) where T : Google.Protobuf.IMessage
        {
            string type = typeof(T).FullName;
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
            string type = typeof(T).FullName;
            if (!delagateMap.ContainsKey(type))
            {
                delagateMap[type] = null;
            }
            delagateMap[type] = (MassageHandler<T>)delagateMap[type] - sender;
        }

        //触发
        void Fire<T>(Connection sender, T msg)
        {
            string type = typeof(T).FullName;
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
        public void AddMessage(Connection sender, Package message)
        {
            lock(messageQueue)
            {
                messageQueue.Enqueue(new MsgUnit { sender = sender, message = message });
            }   
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
            if (isRunning) return;
            isRunning = true;

            this.threadCount = Math.Min(Math.Max(ThreadCount, 1), 200);
            for (int i = 0; i < this.threadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageWorker));
            }
            while (WorkerCount < this.threadCount)
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
                    MsgUnit msgUnit = null;

                    lock (messageQueue) {
                        if (messageQueue.Count == 0)
                        { 
                            continue;
                        }
                        msgUnit = messageQueue.Dequeue();
                    }

                   
                    Google.Protobuf.IMessage package = msgUnit.message;
                    if (package != null)
                    {
                        executeMessage(msgUnit.sender, package);
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

        private void executeMessage(Connection sender, Google.Protobuf.IMessage message)
        {
            //发现消息就触发订阅

            var fireMethod = typeof(MassageRouter).GetMethod("Fire", BindingFlags.NonPublic | BindingFlags.Instance);
            var t = message.GetType();
            var genericMethod = fireMethod.MakeGenericMethod(message.GetType());
            genericMethod.Invoke(this, new object[] { sender, message });
            foreach (var p in t.GetProperties())
            {

                if ("Parser" == p.Name || "Descriptor" == p.Name) continue;
                var value = p.GetValue(message);
                if (value != null)
                {
                    if(typeof(Google.Protobuf.IMessage).IsAssignableFrom(value.GetType()))
                    {
                        //Console.WriteLine("发现消息，触发订阅，继续递归");

                        //继续递归
                        executeMessage(sender, (Google.Protobuf.IMessage)value);
                    }

                }
            }
        }
        ///// <summary>
        ///// 执行消息处理，反射获取消息类型并触发对应的处理函数
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="entity"></param>
        //public void execute(Connection sender, object entity)
        //{
        //    var fireMethod = typeof(MassageRouter).GetMethod("Fire", BindingFlags.NonPublic | BindingFlags.Instance);
        //    Type t = entity.GetType();
        //    foreach (var p in t.GetProperties()) 
        //    {
        //        if ("Parser" == p.Name || "Descriptor" == p.Name) continue;
        //        Console.WriteLine(p.Name);
        //        var value = p.GetValue(entity);//value.GetType == p.PropertyType
        //        Console.WriteLine("====" + value);
        //        if(value != null)
        //        {

        //            var genericMethod = fireMethod.MakeGenericMethod(value.GetType());
        //            genericMethod.Invoke(this, new object[] { sender, value });


        //        }
        //    }

        //}

    }
}