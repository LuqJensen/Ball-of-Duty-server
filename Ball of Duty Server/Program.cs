using Ball_of_Duty_Server.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Map map = new Map();
            map.GameObjects.TryAdd(1, new Character(1));
            map.GameObjects.TryAdd(2, new Character(2));
            Console.ReadLine();
            while (true)
            { 
                map.Update();
                Thread.Sleep(10);
            }
            Console.ReadLine();
        }
    }
}
