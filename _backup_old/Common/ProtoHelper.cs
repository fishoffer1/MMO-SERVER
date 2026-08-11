using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using System.IO;
using System;
using Proto;
using System.Reflection;
using Google.Protobuf.Reflection;
using Serilog;

namespace Summer
{
    /// <summary>
    /// Protobuf序列化与反序列化
    /// </summary>
    public class ProtoHelper
    {
        /// <summary>
        /// 序列化protobuf
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] Serialize(IMessage msg)
        {
            using (MemoryStream rawOutput = new MemoryStream())
            {
                msg.WriteTo(rawOutput);
                byte[] result = rawOutput.ToArray();
                return result;
            }
        }
        /// <summary>
        /// 解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        public static T Parse<T>(byte[] dataBytes) where T : IMessage, new()
        {
            T msg = new T();
            msg = (T)msg.Descriptor.Parser.ParseFrom(dataBytes);
            return msg;
        }


        private static Dictionary<string,Type> _registry = new Dictionary<string,Type>();

        static ProtoHelper()
        {
            var q = from t in Assembly.GetExecutingAssembly().GetTypes() select t;
            q.ToList().ForEach(t =>
            {
                if (typeof(IMessage).IsAssignableFrom(t) && !t.IsAbstract)
                {
                    var desc = t.GetProperty("Descriptor").GetValue(t) as MessageDescriptor;
                    _registry.Add(desc.FullName, t);
                    Log.Information(desc.FullName + " 注册成功");
                }
            });

        }

        public static Package Pack(IMessage msg)
        {
            Package package = new Package();
            package.Fullname = msg.Descriptor.FullName;
            package.Data = ByteString.CopyFrom(Serialize(msg));
            return package;
        }

        public static IMessage Unpack(Package package)
        {
            string fullname = package.Fullname;
            if (_registry.ContainsKey(fullname))
            {
                Type t = _registry[fullname];
                var desc = t.GetProperty("Descriptor").GetValue(t) as MessageDescriptor;
                return desc.Parser.ParseFrom(package.Data.ToByteArray());
            }
            return null;
        }
    }
}
