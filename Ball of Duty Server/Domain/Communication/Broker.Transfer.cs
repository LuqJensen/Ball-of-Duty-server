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

        private ConcurrentDictionary<IPEndPoint, PlayerEndPoint> _playerEndPoints = new ConcurrentDictionary<IPEndPoint, PlayerEndPoint>();

        private ConcurrentDictionary<IPEndPoint, bool> _udpEndPoints = new ConcurrentDictionary<IPEndPoint, bool>();

        private ConcurrentDictionary<AsyncSocket, bool> _connectedClients = new ConcurrentDictionary<AsyncSocket, bool>();

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
            Thread t3 = new Thread(BroadcastTcpAsync);
            t3.Start();
        }

        private void BroadcastTcpAsync()
        {
            while (true)
            {
                byte[] message;
                if (_tcpQueue.TryTake(out message, Timeout.Infinite))
                {
                    foreach (var v in _connectedClients.Keys)
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                await v.SendMessage(message);
                            }
                            catch (SocketException)
                            {
                                RemoveTarget(v);
                            }
                            catch (ObjectDisposedException)
                            {
                            }
                        });
                    }
                }
            }
        }

        public void AddTarget(int playerId, string ip, int udpPort, int tcpPort)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);

            // http://blogs.msdn.com/b/webdev/archive/2013/01/08/dual-mode-sockets-never-create-an-ipv4-socket-again.aspx
            // Socket.RemoteEndPoint property by default returns the IP presented as IPv6
            // So we must make sure that this TCP IP matches it.
            IPEndPoint tcpIpEp = new IPEndPoint(ipAddress.MapToIPv6(), tcpPort);
            // For some reason the IPEndPoint obtained through UdpClient.Receive(ref IPEndPoint)
            // Defaults to IPv4...
            IPEndPoint udpIpEp = new IPEndPoint(ipAddress, udpPort);

            if (_playerEndPoints.TryAdd(tcpIpEp, new PlayerEndPoint(udpIpEp, playerId)))
            {
                _udpEndPoints.TryAdd(udpIpEp, false);
            }
        }

        public void RemoveTarget(AsyncSocket socket)
        {
            bool b;
            _connectedClients.TryRemove(socket, out b);

            PlayerEndPoint endPoint;
            // perhaps add socket to PlayerEndPoint so we can always efficiently remove the player from _connectedClients
            if (_playerEndPoints.TryRemove(socket.IpEndPoint, out endPoint))
            {
                Console.WriteLine($"Client: {socket.IpEndPoint.Address.MapToIPv4()} disconnected.");
                bool b2;
                _udpEndPoints.TryRemove(endPoint.IpEndPoint, out b2);

                Game game;
                if (BoDService.PlayerIngame.TryRemove(endPoint.PlayerId, out game))
                {
                    game.RemovePlayer(endPoint.PlayerId);
                    Console.WriteLine($"Player: {endPoint.PlayerId} quit game: {game.Id}.");
                }
            }
            socket.Dispose();
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
            }
            catch (SocketException)
            {
                socket.Dispose();
                return;
            }

            _connectedClients.TryAdd(s, true);
            Console.WriteLine($"Client connected: {s.IpEndPoint.Address.MapToIPv4()}");

            try
            {
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
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                RemoveTarget(s);
            }
        }

        public void ReceiveUdp()
        {
            while (true)
            {
                IPEndPoint ipEp = null;
                Read(_listener.Receive(ref ipEp));

                _udpEndPoints.TryUpdate(ipEp, true, false);
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