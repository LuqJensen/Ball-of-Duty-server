﻿using Ball_of_Duty_Server.DTO;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System;
using System.Threading;

namespace Ball_of_Duty_Server.Domain
{
    public class Broker : IBroker
    {
        private UdpClient _broadcastSocket;
        private IPEndPoint _ip = new IPEndPoint(IPAddress.Parse("235.1.2.87"), 15000);
        private IPEndPoint _ip2; // Needs new port for each game
        private UdpClient _listener;
        private static int portIncrement = 0;

        public Broker(Map map, string ip)
        {
            Map = map;
            _broadcastSocket = new UdpClient();
            _ip2 = new IPEndPoint(IPAddress.Parse(ip), 15001);
            _listener = new UdpClient(_ip2);
            Thread t = new Thread(Receive);
            t.Start();
        }

        public Map Map { get; set; }

        public void SendPositionUpdate(List<ObjectPosition> positions, int gameId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {

                Thread.Sleep(20);
                bw.Write((byte) 1); //ASCII Standard for Start of heading
                bw.Write((byte) Opcodes.BROADCAST_POSITION_UPDATE);
                bw.Write((byte) 2); //ASCII Standard for Start of text
                for (int i = 0; i < positions.Count; ++i)
                {
                    ObjectPosition op = positions[i];
                    bw.Write(op.Id);
                    bw.Write(op.Position.X);
                    bw.Write(op.Position.Y);
                    if (i != positions.Count - 1)
                    {
                        bw.Write((byte) 31); //ASCII Standard for Unit seperator
                    }
                }
                bw.Write((byte) 4); //ASCII Standard for End of transmission

                Send(ms.ToArray());
            }
        }

        public void Receive()
        {
            while (true)
            {
                byte[] b = _listener.Receive(ref _ip2);
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
            _broadcastSocket.Send(b, b.Length, _ip);
        }
    }
}