using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncSocket;

namespace Ball_of_Duty_Server.Domain.Modules
{
    public static class ModuleManager
    {
        private static AsyncSocketModule _asyncSocketModule = new AsyncSocketModule();
        public static IAsyncSocketFactory AsyncSocketFactory => _asyncSocketModule.AsyncSocketFactory;

        public static void ReloadAll()
        {
            _asyncSocketModule.Reload();
        }
    }
}