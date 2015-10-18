using Ball_of_Duty_Server.DTO;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Ball_of_Duty_Server.Domain
{
    public class Broker : IBroker
    {
        private UdpClient _broadcastSocket;
        private IPEndPoint _ip = new IPEndPoint(/*IPAddress.Broadcast*/IPAddress.Parse("127.0.0.1"), 15000);

        public Broker()
        {
            _broadcastSocket = new UdpClient();
        }
        public void SendPositionUpdate(List<ObjectPosition> positions, int gameId)
        {
            Stream s = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((byte)1); //ASCII Standard for Start of heading
            bw.Write((byte)Opcodes.BROADCAST_POSITION_UPDATE);
            bw.Write((byte)2); //ASCII Standard for Start of text
            for (int i = 0; i < positions.Count; ++i)

            {
                ObjectPosition op = positions[i];
                bw.Write((int)op.Id);
                bw.Write((double)op.Position.X);
                bw.Write((double)op.Position.Y);

                if (i != positions.Count - 1)
                {
                    bw.Write((byte)31); //ASCII Standard for Unit seperator
                }   
            }
            bw.Write((byte)4); //ASCII Standard for End of transmission
            


        }

        public void Send()
        {

        }
    }
}