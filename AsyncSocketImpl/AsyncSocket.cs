﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using AsyncSocket;

namespace AsyncSocketImpl
{
    [Serializable]
    internal partial class AsyncSocket : IAsyncSocket
    {
        public const int BUFFER_LENGTH = 0x10000;

        private bool _disposed = false;
        [NonSerialized] private Socket _socket;
        [NonSerialized] private SocketAwaitableEventWrapper _sender, _receiver;

        public IPEndPoint IpEndPoint { get; }

        public AsyncSocket(Socket s)
        {
            this._socket = s;
            this._socket.NoDelay = true;
            this.IpEndPoint = _socket.RemoteEndPoint as IPEndPoint;
            //this._sender = new SocketAwaitableEventWrapper();
            this._receiver = new SocketAwaitableEventWrapper();
            this._receiver.EventArgs.SetBuffer(new byte[BUFFER_LENGTH], 0, BUFFER_LENGTH);
        }

        public async Task<ReadResult> ReceiveAsync()
        {
            await _socket.ReceiveAsync(_receiver.SocketAwaitable);
            int bytesRead = _receiver.EventArgs.BytesTransferred;

            if (bytesRead <= 0)
            {
                throw new SocketException((int)SocketError.ConnectionAborted);
            }

            byte[] buffer = new byte[bytesRead];
            Array.Copy(_receiver.EventArgs.Buffer, buffer, bytesRead);
            return new ReadResult(bytesRead, buffer);
        }

        public async Task SendMessage(byte[] buffer)
        {
            await Task.Yield();
            // TODO fix this, its bad for performance. Better than before, but we should probably use pooling.
            // The issue is that we dont know exactly how many allocations will be caused by X amount of players
            // making y amount of requests.
            using (SocketAwaitableEventWrapper sender = new SocketAwaitableEventWrapper())
            {
                sender.EventArgs.SetBuffer(buffer, 0, buffer.Length);

                await _socket.SendAsync(sender.SocketAwaitable);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
        protected virtual void Dispose(bool safeToFreeManaged)
        {
            if (!_disposed)
            {
                if (safeToFreeManaged)
                {
                    _socket?.Close();
                    _receiver?.Dispose();
                    _sender?.Dispose();
                }
                // free unmanaged resources here.

                _disposed = true;
            }
        }

        ~AsyncSocket()
        {
            Dispose(false);
        }
    }
}