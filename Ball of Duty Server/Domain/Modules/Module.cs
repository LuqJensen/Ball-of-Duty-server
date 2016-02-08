using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Domain.Modules
{
    public abstract class Module
    {
        private AppDomain _appDomain;
        protected static string _appDomainPath = AppDomain.CurrentDomain.BaseDirectory;

        protected AssemblyLoader ReloadAssembly(string assemblyName, string dllPath)
        {
            if (_appDomain != null)
            {
                AppDomain.Unload(_appDomain);
            }

            byte[] bytes = File.ReadAllBytes(dllPath);

            _appDomain = AppDomain.CreateDomain(assemblyName);
            AssemblyLoader asmLoader = new AssemblyLoader(bytes);
            _appDomain.DoCallBack(asmLoader.LoadAsm);

            return asmLoader;
        }

        public abstract object Factory { get; }

        /// <summary>
        /// The loaded Assembly will be unloaded together with the AppDomain, when a new Assembly is loaded.
        /// </summary>
        public abstract void Reload();
    }
}