using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSocket
{
    public interface IAsyncSocket : IDisposable
    {
        IPEndPoint IpEndPoint { get; }
        Task<ReadResult> ReceiveAsync();
        Task SendMessage(byte[] buffer);
    }
}