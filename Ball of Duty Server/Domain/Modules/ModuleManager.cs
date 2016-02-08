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
        private static Dictionary<Type, Module> _modules = new Dictionary<Type, Module>();

        static ModuleManager()
        {
            _modules.Add(typeof (IAsyncSocketFactory), new AsyncSocketModule());
            _modules.Add(typeof (EntityModule.TempClass), new EntityModule());
        }

        public static void ReloadAll()
        {
            foreach (var v in _modules.Values)
            {
                v.Reload();
            }
        }

        public static T GetModule<T>()
        {
            return (T)_modules[typeof (T)].Factory;
        }
    }
}