// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using Common;

using Common.Proto;
using Google.Protobuf;
using Proto;
using Serilog;
using Summer;
namespace TestClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // 设置日志的最小级别为 Debug
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/Client-log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var address = IPAddress.Parse("127.0.0.1");
            int port = 32510;
            Log.Debug("Hello, World!");
            IPEndPoint ipEndPoint = new(address, port);
            Socket socket = new(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipEndPoint);

            Log.Information ("Connected to server.");
           

            Thread.Sleep(1000);

            Connection conn = new Connection(socket);
            
           

            

            var message = new UserLoginRequest();
            message.Username = "testuserqwb";
            message.Password = "123456";
            conn.Send(message);

            //var pack = ProtoHelper.Pack(message);
            //var res = ProtoHelper.Unpack(pack);
            //Log.Information("{0}：{1}", res.GetType(), res);

            Console.ReadKey();
        }


    }
}