using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AsyncSocket;

namespace AsyncSocketImpl
{
    [Serializable]
    public class AsyncSocketFactory : IAsyncSocketFactory
    {
        public IAsyncSocket CreateAsyncSocket(Socket socket)
        {
            return new AsyncSocket(socket);
        }
    }
}