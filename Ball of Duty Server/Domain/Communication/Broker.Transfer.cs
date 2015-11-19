using Ball_of_Duty_Server.DTO;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain.Maps;
using Ball_of_Duty_Server.Services;
using SocketExtensions;
using Timer = System.Timers.Timer;

namespace Ball_of_Duty_Server.Domain.Communication
{
    public partial class Broker
    {
        private const int SERVER_UDP_PORT = 15001;
        private const int SERVER_TCP_PORT = 15010;
        private const int SIO_UDP_CONNRESET = -1744830452;

        private IPEndPoint _ip; // Needs new port for each game
        private UdpClient _listener;

        private ConcurrentDictionary<IPEndPoint, PlayerEndPoint> _playerEndPoints =
            new ConcurrentDictionary<IPEndPoint, PlayerEndPoint>();

        private ConcurrentDictionary<IPEndPoint, bool> _udpEndPoints =
            new ConcurrentDictionary<IPEndPoint, bool>();

        private ConcurrentDictionary<AsyncSocket, bool> _connectedClients =
            new ConcurrentDictionary<AsyncSocket, bool>();

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
            // http://stackoverflow.com/questions/5199026/c-sharp-async-udp-listener-socketexception
            _listener.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);

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

        private void AddTarget(int playerId, string ip, int udpPort, int tcpPort)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);

            IPEndPoint tcpIpEp = new IPEndPoint(ipAddress, tcpPort);
            IPEndPoint udpIpEp = new IPEndPoint(ipAddress, udpPort);

            if (_playerEndPoints.TryAdd(tcpIpEp, new PlayerEndPoint(udpIpEp, playerId)))
            {
                _udpEndPoints.TryAdd(udpIpEp, false);
            }
        }

        /// <summary>
        /// Removes a target based on its tcp end point.
        /// </summary>
        /// <param name="iPeP"></param>
        public void RemoveTarget(IPEndPoint iPeP)
        {
            PlayerEndPoint endPoint;
            if (_playerEndPoints.TryRemove(iPeP, out endPoint))
            {
                bool b;
                _udpEndPoints.TryRemove(endPoint.IpEndPoint, out b);

                Game game;
                if (BoDService.PlayerIngame.TryRemove(endPoint.PlayerId, out game))
                {
                    game.RemovePlayer(endPoint.PlayerId);
                    //Console.WriteLine($"Player: {id} quit game: {game.Id}.");
                }
            }
        }

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
                Console.WriteLine($"Client connected: {s.IpEndPoint?.ToString().Replace("::ffff:", "")}");

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
                        IPEndPoint ip = s.IpEndPoint;
                        if (ip != null)
                        {
                            Console.WriteLine($"Client: {ip.ToString().Replace("::ffff:", "")} disconnected.");
                            RemoveTarget(ip);
                        }
                    }
                    s.Dispose();
                }
                socket?.Close();
            }
        }


        public void ReceiveUdp()
        {
            while (true)
            {
                IPEndPoint ipEp = null;
                Read(_listener.Receive(ref ipEp));

                if (_udpEndPoints.ContainsKey(ipEp))
                {
                    _udpEndPoints[ipEp] = true;
                }
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
            foreach (var targetEndPoint in _udpEndPoints)
            {
                if (targetEndPoint.Value)
                {
                    _listener.Send(b, b.Length, targetEndPoint.Key);
                }
            }
        }
    }
}