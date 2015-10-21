using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using Ball_of_Duty_Server.Domain;

namespace Ball_of_Duty_Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            /* Map map = new Map();
            map.GameObjects.TryAdd(1, new Character(1));
            map.GameObjects.TryAdd(2, new Character(2));
            while (true)
            { 
                map.Update();
                Thread.Sleep(10);
            }*/

             using (var sh = new ServiceHost(typeof (BoDServer)))
             {
                 ServiceDebugBehavior debug = sh.Description.Behaviors.Find<ServiceDebugBehavior>();

                 // if not found - add behavior with setting turned on 
                 if (debug == null)
                 {
                     sh.Description.Behaviors.Add(
                          new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                 }
                 else
                 {
                     // make sure setting is turned ON
                     if (!debug.IncludeExceptionDetailInFaults)
                     {
                         debug.IncludeExceptionDetailInFaults = true;
                     }
                 }

                 sh.Open();
                 Console.WriteLine("Server is up and running");
                 Console.WriteLine("Press any key to terminate");
                 Console.ReadLine();
                 sh.Close();
             }
        }
    }
}