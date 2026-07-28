// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var address = IPAddress.Parse("127.0.0.1");
            int port = 32510;
            Console.WriteLine("Hello, World!");
            IPEndPoint ipEndPoint = new(address, port);
            Socket socket = new(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipEndPoint);
            Console.WriteLine("Connected to server.");

            string text = "你好啊，服务端";
            //字符串转字节数组
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            sendMessage(socket, bytes);
            sendMessage(socket, Encoding.UTF8.GetBytes("这是第二句话"));
            Console.ReadKey();
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }
                sendMessage(socket, Encoding.UTF8.GetBytes(input));
            }

        }

        public static void sendMessage(Socket socket, byte[] bytes)
        {
            int buffer = bytes.Length;
            byte[] lenbytes = BitConverter.GetBytes(buffer);
            socket.Send(lenbytes);
            socket.Send(bytes);//向服务端发送数据：该数据的长度和本数据。
            Console.WriteLine("成功发送数据：" + Encoding.UTF8.GetString(bytes));
        }
    }
}