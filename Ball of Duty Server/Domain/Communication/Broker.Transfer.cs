using Ball_of_Duty_Server.DTO;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain.Maps;
using Ball_of_Duty_Server.Services;
using Ball_of_Duty_Server.Utility;
using SocketExtensions;

namespace Ball_of_Duty_Server.Domain.Communication
{
    public partial class Broker
    {
        private const int SERVER_UDP_PORT = 15001;
        private const int SERVER_TCP_PORT = 15010;
        private const int SIO_UDP_CONNRESET = -1744830452;

        private const int TCP_TIMEOUT = 10000;
        private const int SESSIONID_LENGTH = 32;

        private IPEndPoint _ip; // Needs new port for each game
        private UdpClient _listener;

        private ConcurrentDictionary<IPEndPoint, PlayerEndPoint> _playerEndPoints = new ConcurrentDictionary<IPEndPoint, PlayerEndPoint>();
        private ConcurrentDictionary<string, PlayerEndPoint> _playerSessionTokens = new ConcurrentDictionary<string, PlayerEndPoint>();

        /// <summary>
        /// Pairs an AsyncSocket with a TCP read timeout LightEvent.
        /// </summary>
        private ConcurrentDictionary<AsyncSocket, LightEvent> _connectedClients = new ConcurrentDictionary<AsyncSocket, LightEvent>();

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

        public void Update(long deltaTime)
        {
            foreach (var v in _connectedClients)
            {
                v.Value.Update(deltaTime);
                PlayerEndPoint playerEndPoint;
                if (_playerEndPoints.TryGetValue(v.Key.IpEndPoint, out playerEndPoint))
                {
//                    playerEndPoint.InactivityEvent?.Update(deltaTime);
                }
            }
        }


        public void RemoveTarget(AsyncSocket socket)
        {
            LightEvent e;
            _connectedClients.TryRemove(socket, out e);

            PlayerEndPoint endPoint;
            // perhaps add socket to PlayerEndPoint so we can always efficiently remove the player from _connectedClients
            if (_playerEndPoints.TryRemove(socket.IpEndPoint, out endPoint))
            {
                BoDConsole.WriteLine($"Client: {socket.IpEndPoint.Address.MapToIPv4()}:{socket.IpEndPoint.Port} disconnected.");

                // If any PlayerEndPoint, other than the one that was just removed, 
                // references PlayerId, the player must be connected on another PlayerEndPoint.
                if (_playerEndPoints.Values.All(p => p.PlayerId != endPoint.PlayerId))
                {
                    Game game;
                    if (BoDService.PlayerIngame.TryRemove(endPoint.PlayerId, out game))
                    {
                        game.RemovePlayer(endPoint.PlayerId);
                    }
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

        /// <summary>
        /// Generates a cryptographically random sessionId.
        /// And associates it with a PlayerEndPoint containing,
        /// this sessionId and the clients playerId.
        /// </summary>
        /// <param name="playerId"> The playerId of the client requesting a sessionId. </param>
        /// <param name="ip"> The IP from which the client makes the request. </param>
        /// <returns> A cryptographically random sessionId. </returns>
        public byte[] GenerateSessionId(int playerId, string ip)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            byte[] randomBytes = new byte[SESSIONID_LENGTH].FillRandomly();
            string sessionId = Convert.ToBase64String(randomBytes);

            _playerSessionTokens.TryAdd(sessionId, new PlayerEndPoint(playerId, sessionId));

            return randomBytes;
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

            // Cancel the attempt to communicate with the client on both TCP and UDP after 10 sec.
            CancellationTokenSource cts = new CancellationTokenSource(TCP_TIMEOUT);
            Task timeout = new Task(async () =>
            {
                await Task.Delay(TCP_TIMEOUT);
                throw new SocketException();
            });

            try
            {
                Task<bool> tcp = AddTCPTarget(s);
                // Only await timeout once. If tcp finishes first we wont get an exception when timeout finishes.
                await Task.WhenAny(tcp, timeout);

                // await tcp here aswell because we want the result of it. Once we got the result of AddTCPTarget(),
                // we await the result of AddUDPTarget.
                PlayerEndPoint playerEndPoint;
                if (await tcp && _playerEndPoints.TryGetValue(s.IpEndPoint, out playerEndPoint) && await AddUDPTarget(playerEndPoint, cts.Token))
                {
                    // Timeout the TCP and UDP connection to a client if we dont receive a TCP message
                    // at least once per 10 sec.
                    _connectedClients.TryAdd(s, new LightEvent(TCP_TIMEOUT, () => { RemoveTarget(s); }));
                    playerEndPoint.InactivityEvent = new LightEvent(60000, () => { RemoveTarget(s); });

                    BoDConsole.WriteLine($"Client connected: {s.IpEndPoint.Address.MapToIPv4()}");
                    // Start reading actual game related messages from the client.
                    await ReceiveFromClientAsync(s);
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
                cts.Dispose();
                RemoveTarget(s);
            }
        }

        /// <summary>
        /// Awaits a message from the client containing a sessionId.
        /// If the sessionId matches any we have given out. Assign the found PlayerEndPoint
        /// to the client. We write back the sessionId again as a signal that the client should
        /// begin attempting to "connect" with UDP.
        /// </summary>
        /// <param name="socket"> The TCP socket for the client. </param>
        /// <returns> true if the client succesfully verified through TCP. false otherwise. </returns>
        private async Task<bool> AddTCPTarget(AsyncSocket socket)
        {
            AsyncSocket.ReadResult result = await socket.ReceiveAsync();
            if (result.BytesRead != SESSIONID_LENGTH)
            {
                return false;
            }

            string sessionId = Convert.ToBase64String(result.Buffer);
            PlayerEndPoint playerEndPoint;
            if (!_playerSessionTokens.TryGetValue(sessionId, out playerEndPoint))
            {
                return false;
            }

            if (sessionId != playerEndPoint.SessionId)
                return false;

            playerEndPoint.TCPSocket = socket;

            await socket.SendMessage(result.Buffer);

            return _playerEndPoints.TryAdd(socket.IpEndPoint, playerEndPoint);
        }

        /// <summary>
        /// Continuously awaits a UDP message from the client,
        /// having opcode == Opcode.UDP_CONNECT and containing a matching sessionId.
        /// When playerEndPoint.UdpIpEndPoint != null the client has succesfully identified
        /// with UDP. We then send the sessionId once more to tell the client to stop sending
        /// further UDP packets.
        /// </summary>
        /// <param name="playerEndPoint"> A token identifying and linking the client to its UDP and TCP "connections". </param>
        /// <param name="ct"> A CancellationToken that will expire after 10 seconds. </param>
        /// <returns> true if the client succesfully verified through UDP. false otherwise. </returns>
        private async Task<bool> AddUDPTarget(PlayerEndPoint playerEndPoint, CancellationToken ct)
        {
            while (playerEndPoint.UdpIpEndPoint == null)
            {
                if (ct.IsCancellationRequested)
                {
                    return false;
                }

                await Task.Delay(100);
            }

            await playerEndPoint.TCPSocket.SendMessage(Convert.FromBase64String(playerEndPoint.SessionId));

            return (_playerSessionTokens.TryRemove(playerEndPoint.SessionId, out playerEndPoint));
        }

        private async Task ReceiveFromClientAsync(AsyncSocket s)
        {
            while (true)
            {
                Task<AsyncSocket.ReadResult> receive = s.ReceiveAsync();
                await receive;
                LightEvent e;
                if (_connectedClients.TryGetValue(s, out e))
                {
                    // Reset the LightEvent's timer to prevent timeout.
                    e.Reset();
                }

                Read(receive.Result.Buffer, s.IpEndPoint, s);
            }
        }

        public void ReceiveUdp()
        {
            while (true)
            {
                IPEndPoint ipEp = null;
                Read(_listener.Receive(ref ipEp), ipEp, null);
            }
        }

        public void BroadcastTcp(byte[] b)
        {
            _tcpQueue.TryAdd(b);
        }

        private void SendTcpTo(byte[] b, AsyncSocket s)
        {
            Task.Run(async () =>
            {
                try
                {
                    await s.SendMessage(b);
                }
                catch (SocketException)
                {
                    RemoveTarget(s);
                }
                catch (ObjectDisposedException)
                {
                }
            });
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

        public void SendUdp(byte[] b)
        {
            foreach (var targetEndPoint in _playerEndPoints.Values)
            {
                if (targetEndPoint.UdpIpEndPoint != null)
                {
                    _listener.Send(b, b.Length, targetEndPoint.UdpIpEndPoint);
                }
            }
        }
    }
}