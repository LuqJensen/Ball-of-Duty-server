using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSocket
{
    public interface IAsyncSocketFactory
    {
        IAsyncSocket CreateAsyncSocket(Socket socket);
    }
}