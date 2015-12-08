using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Utility
{
    internal static class BoDConsole
    {
        public static void WriteLine(String s)
        {
            Console.WriteLine($"[{DateTime.Now.TimeOfDay.ToString("hh\\:mm\\:ss")}] {s}");
        }
    }
}