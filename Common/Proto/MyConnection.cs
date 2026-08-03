using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common.Proto
{
    public class MyConnection : Connection
    {
        public MyConnection(Socket socket) : base(socket)
        {

        }
        private Package _package = null;

        public Request Request
        {
            get
            {
                if (_package == null)
                {
                    _package = new Package();
                }
                if (_package.Request == null)
                {
                    _package.Request = new Request();
                }
                return _package.Request;
            }
        }

        public Response Response
        {
            get
            {
                if (_package == null)
                {
                    _package = new Package();
                }
                if (_package.Response == null)
                {
                    _package.Response = new Response();
                }
                return _package.Response;
            }
        }

        public void Send()
        {
            if (_package != null) Send(_package);
            _package = null;
        }
    }
}
