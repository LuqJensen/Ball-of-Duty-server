using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSocketImpl
{
    internal partial class AsyncSocket
    {
        private class SocketAwaitableEventWrapper : IDisposable
        {
            private bool _disposed = false;

            public SocketAsyncEventArgs EventArgs { get; }
            public SocketAwaitable SocketAwaitable { get; }

            public SocketAwaitableEventWrapper()
            {
                EventArgs = new SocketAsyncEventArgs();
                SocketAwaitable = new SocketAwaitable(EventArgs);
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
                        EventArgs?.Dispose();
                    }
                    // free unmanaged resources here.

                    _disposed = true;
                }
            }

            ~SocketAwaitableEventWrapper()
            {
                Dispose(false);
            }
        }
    }
}