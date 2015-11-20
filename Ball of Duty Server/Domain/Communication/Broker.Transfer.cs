using Ball_of_Duty_Server.DTO;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Ball_of_Duty_Server.DAO;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Domain.Maps;
using Ball_of_Duty_Server.Services;
using SocketExtensions;
using Timer = System.Timers.Timer;

namespace Ball_of_Duty_Server.Domain.Communication
{
    public partial class Broker
    {
        private UdpClient _udpBroadcastSocket;
        private IPEndPoint _ip; // Needs new port for each game
        private UdpClient _listener;
        private ConcurrentDictionary<int, IPEndPoint> _targetEndPoints = new ConcurrentDictionary<int, IPEndPoint>();
        private ConcurrentDictionary<string, int> _IpToPlayerId = new ConcurrentDictionary<string, int>();

        private ConcurrentDictionary<string, byte> _activatedTargets = new ConcurrentDictionary<string, byte>();
        // Checks if a client has send a udp packet

        private ConcurrentDictionary<AsyncSocket, bool> _connectedClients =
            new ConcurrentDictionary<AsyncSocket, bool>();

        private const int SERVER_UDP_PORT = 15001;
        private const int SERVER_TCP_PORT = 15010;
        private TcpListener _tcpListener = TcpListener.Create(SERVER_TCP_PORT); // TODO dynamic port

        private BlockingCollection<byte[]> _tcpQueue = new BlockingCollection<byte[]>();

        private static int portIncrement = 0;
        public Map Map { get; set; }

        public Broker(Map map)
        {
            Map = map;
            AddOpcodeMapping();
            _tcpListener.Start();
            _ip = new IPEndPoint(IPAddress.Any, SERVER_UDP_PORT);
            _listener = new UdpClient(_ip);

            Thread t = new Thread(ReceiveUdp);
            t.Start();
            Thread t2 = new Thread(() => { AcceptClientsAsync().Wait(); });
            t2.Start();
            Thread t3 = new Thread(() => { BroadcastTcpAsync().Wait(); });
            t3.Start();
        }

        private Task BroadcastTcpAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    byte[] message;
                    if (_tcpQueue.TryTake(out message, Timeout.Infinite))
                    {
                        foreach (var v in _connectedClients.Keys)
                        {
                            try
                            {
                                await v.SendMessage(message);
                                // TODO find out if each iteration is waiting for "ack" from the client.
                            }
                            catch (SocketException)
                            {
                                v.Dispose();
                            }
                            catch (ObjectDisposedException ex)
                            {
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                    }
                }
            });
        }

        private void AddTarget(int playerId, string ip, int preferedPort, int tcpPort)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), preferedPort);
            _targetEndPoints.TryAdd(playerId, ipep);
            IPEndPoint ipepTcp = new IPEndPoint(IPAddress.Parse(ip), tcpPort);

            _IpToPlayerId.TryAdd(ipepTcp.ToString(), playerId);
        }

        /// <summary>
        /// Removes a target based on its tcp end point.
        /// </summary>
        /// <param name="ipep"></param>
        public void RemoveTarget(string ipep)
        {
            int id;
            _IpToPlayerId.TryRemove(ipep, out id);

            Game game;
            if (BoDService.PlayerIngame.TryRemove(id, out game))
            {
                game.RemovePlayer(id);
//                Console.WriteLine($"Player: {id} quit game: {game.Id}.");
            }
        }

        /// <summary>
        /// Removes a target based on its player id.
        /// </summary>
        /// <param name="ipep"></param>
        public void RemoveTarget(int id)
        {
            IPEndPoint ipep;
            byte dump1;
            int dump2;
            _targetEndPoints.TryRemove(id, out ipep);
            _activatedTargets.TryRemove(ipep.ToString(), out dump1);
            _IpToPlayerId.TryRemove(ipep.ToString(), out dump2);
        }

        private bool _receivedUdp = false;


        private Task AcceptClientsAsync()
        {
            return Task.Run(async () =>
            {
                while (true) // TODO while less than max clients
                {
                    TcpClient client = null;
                    try
                    {
                        client = await _tcpListener.AcceptTcpClientAsync();
                        Task t = AcceptClientAsync(client.Client);

                        if (t.IsFaulted)
                            t.Wait();
                    }
                    catch (SocketException)
                    {
                        client?.Close();
                    }
                }
            });
        }

        private async Task AcceptClientAsync(Socket socket)
        {
            await Task.Yield();

            AsyncSocket s = null;
            try
            {
                s = new AsyncSocket(socket);
                _connectedClients.TryAdd(s, true);
                Console.WriteLine("Client connected: " + s.GetIpAddress());

                while (true)
                {
                    Task<AsyncSocket.ReadResult> receive = s.ReceiveAsync();
                    await receive;
                    ReceiveTcp(receive.Result);
                }
            }
            catch (SocketException)
            {
            }
            finally
            {
                if (s != null)
                {
                    bool b;
                    if (_connectedClients.TryRemove(s, out b))
                    {
                        string ip = s.GetIpAddress().Replace("::ffff:", "");
                        Console.WriteLine($"Client: {ip} disconnected.");
                        RemoveTarget(ip);
                    }
                    s.Dispose();
                }
                socket?.Close();
            }
        }


        public void ReceiveUdp()
        {
            try
            {
                while (true)
                {
                    Read(_listener.Receive(ref _ip));
                    _activatedTargets.AddOrUpdate(_ip.ToString(), 0, (key, oldValue) => 0);
                }
            }
            catch (SocketException)
            {
                ReceiveUdp();
            }
        }

        public void ReceiveTcp(AsyncSocket.ReadResult rr)
        {
            Read(rr.Buffer);
        }

        public void SendTcp(byte[] b)
        {
            _tcpQueue.TryAdd(b);
        }

        public void SendUdp(byte[] b)
        {
            foreach (IPEndPoint targetEndPoint in _targetEndPoints.Values)
            {
                if (_activatedTargets.ContainsKey(targetEndPoint.ToString()))
                {
                    _listener.Send(b, b.Length, targetEndPoint);
                }
            }
        }
    }
}