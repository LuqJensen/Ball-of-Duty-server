using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketExtensions
{
    /// <summary>
    /// @Author: Stephen Toub - Microsoft Visual Studio team MVP
    /// Socketawaitable provides awaitable socket operations along with reusable
    /// SocketAsyncEventArgs. Socketawaitable provides an extreme high-performance
    /// and great scaling wrapper compared to standard async socket operations.
    /// http://blogs.msdn.com/b/pfxteam/archive/2011/12/15/10248293.aspx
    /// https://social.msdn.microsoft.com/Profile/stephen%20toub%20-%20msft
    /// https://github.com/stephentoub
    /// </summary>
    public sealed class SocketAwaitable : INotifyCompletion
    {
        private static readonly Action SENTINEL = () => { };

        internal bool m_wasCompleted;
        internal Action m_continuation;
        internal SocketAsyncEventArgs m_eventArgs;

        public SocketAwaitable(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException("eventArgs");
            m_eventArgs = eventArgs;
            eventArgs.Completed += delegate
            {
                var prev = m_continuation ?? Interlocked.CompareExchange(
                    ref m_continuation, SENTINEL, null);
                if (prev != null)
                    prev();
            };
        }

        internal void Reset()
        {
            m_wasCompleted = false;
            m_continuation = null;
        }

        public SocketAwaitable GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted
        {
            get { return m_wasCompleted; }
        }

        public void OnCompleted(Action continuation)
        {
            if (m_continuation == SENTINEL ||
                Interlocked.CompareExchange(
                    ref m_continuation, continuation, null) == SENTINEL)
            {
                Task.Run(continuation);
            }
        }

        public void GetResult()
        {
            if (m_eventArgs.SocketError != SocketError.Success)
                throw new SocketException((int)m_eventArgs.SocketError);
        }
    }
}