using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using Ball_of_Duty_Server.Persistence;
using Ball_of_Duty_Server.Services;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server
{
    internal class Program
    {
        public enum Command
        {
            NONE,
            WRITE_ACTIVE_PLAYERS,
            BAN,
            SERVER_MESSAGE
        }


        public static Command GetCommandType(string line)
        {
            if (line.ToLower().StartsWith("sm") || line.ToLower().StartsWith("server_message"))
            {
                return Command.SERVER_MESSAGE;
            }
            if (line.ToLower().StartsWith("active_players"))
            {
                return Command.WRITE_ACTIVE_PLAYERS;
            }
            if (line.ToLower().StartsWith("ban"))
            {
                return Command.BAN;
            }
            return Command.NONE;
        }

        public static string GetCommand(string line)
        {
            return line.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase) < 0
                ? "" : line.Substring(line.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase) + 1);
        }

        private static void Main(string[] args)
        {
            //            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            //            {
            //                //Console.WriteLine(RSA.KeySize);
            //                var v = RSA.ExportParameters(true);
            //                //Console.WriteLine(v.Exponent.Length); // uint16 (short) 65537
            //                foreach (var b in v.Modulus)
            //                {
            //                    Console.Write(b);
            //                }
            //                Console.WriteLine();
            //                foreach (var b in v.Exponent)
            //                {
            //                    Console.Write(b);
            //                }
            //                Console.WriteLine();
            //                //Console.WriteLine(RSA.KeySize);
            //            }

            using (var sh = new ServiceHost(typeof (BoDService)))
            {
                /*ServiceDebugBehavior debug = sh.Description.Behaviors.Find<ServiceDebugBehavior>();

                // if not found - add behavior with setting turned on 
                if (debug == null)
                {
                    sh.Description.Behaviors.Add(
                        new ServiceDebugBehavior()
                        {
                            IncludeExceptionDetailInFaults = true
                        });
                }
                else
                {
                    // make sure setting is turned ON
                    if (!debug.IncludeExceptionDetailInFaults)
                    {
                        debug.IncludeExceptionDetailInFaults = true;
                    }
                }*/

                sh.Open();

                BoDConsole.WriteLine("Server is up and running");

                while (true)
                {
                    var lineRead = Console.ReadLine();
                    Command commandType = GetCommandType(lineRead);
                    switch (commandType)
                    {
                        case Command.SERVER_MESSAGE:
                        {
                            BoDService.WriteServerMessage(GetCommand(lineRead));
                            break;
                        }
                        case Command.WRITE_ACTIVE_PLAYERS:
                        {
                            break; // TODO
                        }
                        case Command.BAN:
                        {
                            break; // TODO
                        }
                    }
                }
            }
        }
    }
}