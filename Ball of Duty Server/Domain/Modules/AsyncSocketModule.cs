using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using AsyncSocket;

namespace Ball_of_Duty_Server.Domain.Modules
{
    public class AsyncSocketModule : Module
    {
        private const string ASSEMBLY_NAME = "AsyncSocketImpl";
        private const string FULLY_QUALIFIED_TYPE_NAME = ASSEMBLY_NAME + ".AsyncSocketFactory";
        private static string _dllPath = Path.Combine(_appDomainPath, $@"{ASSEMBLY_NAME}.dll");
        private IAsyncSocketFactory _asyncSocketFactory;

        public override object Factory => _asyncSocketFactory;

        public AsyncSocketModule()
        {
            Reload();
        }

        public override sealed void Reload()
        {
            _asyncSocketFactory = (IAsyncSocketFactory)base.ReloadAssembly(ASSEMBLY_NAME, _dllPath).Assembly.CreateInstance(FULLY_QUALIFIED_TYPE_NAME);
        }
    }
}