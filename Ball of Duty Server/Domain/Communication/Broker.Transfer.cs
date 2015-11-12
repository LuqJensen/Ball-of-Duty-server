using Ball_of_Duty_Server.DTO;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Ball_of_Duty_Server.DAO;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Domain.Maps;
using SocketExtensions;

namespace Ball_of_Duty_Server.Domain.Communication
{
    public partial class Broker
    {
        private UdpClient _udpBroadcastSocket;
        private IPEndPoint _ip; // Needs new port for each game
        private UdpClient _listener;
        private ConcurrentDictionary<int, IPEndPoint> _targetEndPoints = new ConcurrentDictionary<int, IPEndPoint>();

        private ConcurrentDictionary<AsyncSocket, bool> _connectedClients =
            new ConcurrentDictionary<AsyncSocket, bool>();

        private TcpListener _tcpListener = TcpListener.Create(15010); // TODO dynamic port

        private BlockingCollection<byte[]> _tcpQueue = new BlockingCollection<byte[]>();

        private static int portIncrement = 0;
        public Map Map { get; set; }

        public Broker(Map map)
        {
            Map = map;
            AddOpcodeMapping();
            _tcpListener.Start();
            _udpBroadcastSocket = new UdpClient();
            _ip = new IPEndPoint(IPAddress.Any, 15001);
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

        private void AddTarget(int playerId, string ip, int preferedPort)
        {
            _targetEndPoints.TryAdd(playerId, new IPEndPoint(IPAddress.Parse(ip), preferedPort));
        }

        public void RemoveTarget(int id)
        {
            IPEndPoint ip;
            _targetEndPoints.TryRemove(id, out ip);
        }

        public void ReceiveUdp()
        {
            while (true)
            {
                Read(_listener.Receive(ref _ip));
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
                Console.WriteLine("Someone disconnected");
            }
            finally
            {
                if (s != null)
                {
                    bool b;
                    if (_connectedClients.TryRemove(s, out b))
                    {
                        Console.WriteLine($"Client: {s.GetIpAddress()} disconnected.");
                    }
                    s.Dispose();
                }
                socket?.Close();
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
                _udpBroadcastSocket.Send(b, b.Length, targetEndPoint);
            }
        }
    }
}