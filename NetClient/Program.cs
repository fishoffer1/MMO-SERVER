// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using Common.Network;
using Google.Protobuf;
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

            Thread.Sleep(1000);

            NetConnection conn = new NetConnection(socket, null, null);
            //构建发送
            //Package package = new Package();
            //package.Request = new Request();
            //package.Request.UserLogin = new UserLoginRequest();
            //package.Request.UserLogin.Username = "testuser";
            //package.Request.UserLogin.Password = "123456";
            //conn.Send(package);
            //快捷发送
            conn.Request.UserLogin = new UserLoginRequest();
            conn.Request.UserLogin.Username = "hero";
            conn.Request.UserLogin.Password = "666666";
            conn.Send();
            

            Console.ReadKey();
        }

        //public static void sendMessage(Socket socket, byte[] bytes)
        //{
        //    int buffer = bytes.Length;
        //    byte[] lenbytes = BitConverter.GetBytes(buffer);
        //    socket.Send(lenbytes);
        //    socket.Send(bytes);//向服务端发送数据：该数据的长度和本数据。
        //    Console.WriteLine("成功发送数据：" + Encoding.UTF8.GetString(bytes));
        //}
    }
}