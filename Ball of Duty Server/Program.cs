﻿using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using Ball_of_Duty_Server.Domain;
using System.Security.Cryptography;
using System.Text;
using Ball_of_Duty_Server.Persistence;

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

            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                //Console.WriteLine(RSA.KeySize);
                var v = RSA.ExportParameters(true);
                //Console.WriteLine(v.Exponent.Length); // uint16 (short) 65537
                foreach (var b in v.Modulus)
                {
                    Console.Write(b);
                }
                Console.WriteLine();
                foreach (var b in v.Exponent)
                {
                    Console.Write(b);
                }
                Console.WriteLine();
                //Console.WriteLine(RSA.KeySize);
            }

            using (BoDServerEntities bse = new BoDServerEntities())
            {
                for (int i = 0; i < 10; ++i)
                {
                    bse.Players.Add(new Player() {Nickname = $"{i}"});
                }
                bse.SaveChanges();
            }

            using (BoDServerEntities bse = new BoDServerEntities())
            {
                foreach (var v in bse.Players)
                {
                    Console.WriteLine(v.Id);
                }
            }

            using (var sh = new ServiceHost(typeof(BoDService)))
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