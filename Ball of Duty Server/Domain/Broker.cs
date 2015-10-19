using Ball_of_Duty_Server.DTO;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    public class Broker : IBroker
    {
        private UdpClient _broadcastSocket;
        private IPEndPoint _ip = new IPEndPoint(/*IPAddress.Broadcast*/IPAddress.Parse("127.0.0.1"), 15000);

        public Broker(Map map)
        {
            Map = map;
            _broadcastSocket = new UdpClient(_ip);
            
        }

        public Map Map { get; set; }

        public void SendPositionUpdate(List<ObjectPosition> positions, int gameId)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
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
            
            Send(ms.ToArray());
        }

        public void Receive()
        {
            while (true)
            {
                byte[] b =_broadcastSocket.Receive(ref _ip);
                using (MemoryStream ms = new MemoryStream(b))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    if (br.ReadByte() != 1)
                    {
                        return;
                    }
                    Opcodes opcode = (Opcodes)br.ReadByte();
                    br.ReadByte();

                    int id = br.ReadInt32();
                    double x = br.ReadDouble();
                    double y = br.ReadDouble();

                    Map.UpdatePosition(new Point(x, y), id);
                }
            }
        }



        public void Send(byte[] b)
        {
            _broadcastSocket.Send(b, b.Length);
        }
    }
}