using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Domain.Modules
{
    public class AssemblyLoader : MarshalByRefObject
    {
        private byte[] Data { get; }
        public Assembly Assembly { get; private set; }

        public AssemblyLoader(byte[] asmByteCode)
        {
            Data = asmByteCode;
        }

        public void LoadAsm()
        {
            Assembly = Assembly.Load(Data);
        }
    }
}