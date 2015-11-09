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
using SocketExtensions;

namespace Ball_of_Duty_Server.Domain
{
    public class Broker : IBroker
    {
        private UdpClient _udpBroadcastSocket;
        private IPEndPoint _ip; // Needs new port for each game
        private UdpClient _listener;
        private ConcurrentDictionary<int, IPEndPoint> _targetEndPoints = new ConcurrentDictionary<int, IPEndPoint>();
        private ConcurrentBag<AsyncSocket> _connectedClients = new ConcurrentBag<AsyncSocket>();
        // TODO: make use of this.
        private TcpListener _tcpListener = TcpListener.Create(15010);

        private readonly Dictionary<Opcodes, Action<BinaryReader>> _opcodeMapping =
            new Dictionary<Opcodes, Action<BinaryReader>>();

        private static int portIncrement = 0;
        public Map Map { get; set; }

        public Broker(Map map)
        {
            _opcodeMapping.Add(Opcodes.POSITION_UPDATE, this.ReadPositionUpdate);
            _opcodeMapping.Add(Opcodes.REQUEST_BULLET, this.BulletCreationRequest);
            _tcpListener.Start();
            Map = map;
            _udpBroadcastSocket = new UdpClient();
            _ip = new IPEndPoint(IPAddress.Any, 15001);
            _listener = new UdpClient(_ip);
            Thread t = new Thread(Receive);
            t.Start();
            Thread t2 = new Thread(() => { AcceptClientsAsync().Wait(); });
            t2.Start();
        }

        private void BulletCreationRequest(BinaryReader reader)
        {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            double radius = reader.ReadDouble();
            double velocityX = reader.ReadDouble();
            double velocityY = reader.ReadDouble();
            int bulletType = reader.ReadInt32();
            double damage = reader.ReadDouble();
            int ownerId = reader.ReadInt32();
            int entityType = reader.ReadInt32();

            int bulletId = Map.AddBullet(x, y, radius, damage, ownerId);
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)1); //ASCII Standard for Start of heading
                bw.Write((byte)Opcodes.REQUEST_BULLET);
                bw.Write((byte)2); //ASCII Standard for Start of text
                bw.Write(x);
                bw.Write(y);
                bw.Write(radius);
                bw.Write(velocityX);
                bw.Write(velocityY);
                bw.Write(bulletType);
                bw.Write(damage);
                bw.Write(ownerId);
                bw.Write(bulletId);
                bw.Write(entityType);

                bw.Write((byte)4); //ASCII Standard for End of transmission
                SendTcp(ms.ToArray());
            }
        }

        private void ReadPositionUpdate(BinaryReader reader)
        {
            do
            {
                int id = reader.ReadInt32();
                double x = reader.ReadDouble();

                double y = reader.ReadDouble();

                Map.UpdatePosition(new Point(x, y), id);
            } while (reader.Read() == 31);
        }

        public void AddPlayer(int playerId, GameObjectDAO charData, string ip, int preferedPort)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)1); //ASCII Standard for Start of heading
                bw.Write((byte)Opcodes.NEW_PLAYER);
                bw.Write((byte)2); //ASCII Standard for Start of text
                bw.Write(playerId);
                bw.Write(charData.Id);
                bw.Write(charData.X);
                bw.Write(charData.Y);
                bw.Write(charData.Width);
                bw.Write(charData.Height);
                bw.Write(0); // Temporary till EntityType enum is implemented on server.

                bw.Write((byte)4); //ASCII Standard for End of transmission
                SendTcp(ms.ToArray());
            }

            addTarget(playerId, ip, preferedPort);
        }

        private void addTarget(int playerId, string ip, int preferedPort)
        {
            _targetEndPoints.TryAdd(playerId, new IPEndPoint(IPAddress.Parse(ip), preferedPort));
        }

        public void RemovePlayer(int playerId, int characterId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)1); //ASCII Standard for Start of heading
                bw.Write((byte)Opcodes.DISCONNECTED_PLAYER);
                bw.Write((byte)2); //ASCII Standard for Start of text
                bw.Write(playerId);
                bw.Write(characterId);

                bw.Write((byte)4); //ASCII Standard for End of transmission
                SendTcp(ms.ToArray());
            }

            RemoveTarget(playerId);
        }

        public void RemoveTarget(int id)
        {
            IPEndPoint ip;
            _targetEndPoints.TryRemove(id, out ip);
        }

        public void SendPositionUpdate(List<ObjectPosition> positions, int gameId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)1); //ASCII Standard for Start of heading
                bw.Write((byte)Opcodes.BROADCAST_POSITION_UPDATE);
                bw.Write((byte)2); //ASCII Standard for Start of text
                for (int i = 0; i < positions.Count; ++i)
                {
                    ObjectPosition op = positions[i];
                    bw.Write(op.Id);
                    bw.Write(op.Position.X);
                    bw.Write(op.Position.Y);
                    if (i != positions.Count - 1)
                    {
                        bw.Write((byte)31); //ASCII Standard for Unit seperator
                    }
                }
                bw.Write((byte)4); //ASCII Standard for End of transmission

                SendUdp(ms.ToArray());
            }
        }

        public void Receive()
        {
            while (true)
            {
                byte[] b = _listener.Receive(ref _ip);
                Read(ref b);
            }
        }

        private async Task AcceptClientsAsync()
        {
            await Task.Run(async () =>
            {
                await Task.Yield();
                while (true) // TODO while less than max clients
                {
                    AcceptClientAsync();
                }
            });
        }

        private async void AcceptClientAsync()
        {
            AsyncSocket s = null;
            try
            {
                var v = await _tcpListener.AcceptTcpClientAsync();
                s = new AsyncSocket(v.Client);
                Console.WriteLine("Client connected: " + s.GetIpAddress());
                _connectedClients.Add(s);

                try
                {
                    while (true)
                    {
                        ReceiveTcp(await s.ReceiveAsync());
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Client disconnected: " + s.GetIpAddress());
                    _connectedClients.TryTake(out s);
                }
            }
            catch (SocketException e)
            {
                if (s != null)
                {
                    Console.WriteLine("Client disconnected: " + s.GetIpAddress());
                    _connectedClients.TryTake(out s);
                }
            }
            catch (EndOfStreamException e)
            {
                if (s != null)
                {
                    Console.WriteLine("Client disconnected: " + s.GetIpAddress());
                    _connectedClients.TryTake(out s);
                }
            }
        }

        public void ReceiveTcp(AsyncSocket.ReadResult rr)
        {
            Read(ref rr.Buffer);
        }

        private void Read(ref byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                if (br.ReadByte() != 1)
                {
                    return;
                }

                Opcodes opcode = (Opcodes)br.ReadByte();
                br.ReadByte();

                Action<BinaryReader> readMethod;
                if (_opcodeMapping.TryGetValue(opcode, out readMethod))
                {
                    readMethod(br);
                }
            }
        }

        public void SendTcp(byte[] b)
        {
            foreach (AsyncSocket client in _connectedClients)
            {
                client.SendMessage(b);
            }
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