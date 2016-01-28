using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ball_of_Duty_Server.DAO;

namespace Ball_of_Duty_Server.Domain.Communication
{
    public partial class Broker
    {
        private readonly Dictionary<Opcodes, Action<BinaryReader>> _opcodeMapping = new Dictionary<Opcodes, Action<BinaryReader>>();

        private void AddOpcodeMapping()
        {
            _opcodeMapping.Add(Opcodes.POSITION_UPDATE, this.ReadPositionUpdate);
            _opcodeMapping.Add(Opcodes.REQUEST_BULLET, this.BulletCreationRequest);
        }

        private void HandlePing(BinaryReader br, IPEndPoint endPoint)
        {
            br.ReadByte(); // ASCII.EOT

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((uint)Opcodes.PING);
                bw.Write((byte)ASCII.STX);

                bw.Write((byte)ASCII.EOT);
                SendTcpTo(ms.ToArray(), endPoint);
            }
        }


        private void Read(byte[] buffer, IPEndPoint endPoint)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(buffer));
            try
            {
                while (br.ReadByte() == (byte)ASCII.SOH)
                {
                    Opcodes opcode = (Opcodes)br.ReadUInt32();

                    br.ReadByte();

                    if (opcode == Opcodes.UDP_CONNECT)
                    {
                        PlayerEndPoint playerEndPoint;
                        if (_playerSessionTokens.TryGetValue(Convert.ToBase64String(br.ReadBytes(SESSIONID_LENGTH)), out playerEndPoint))
                        {
                            playerEndPoint.UdpIpEndPoint = endPoint;
                        }
                        return;
                    }

                    if (Opcodes.TCP_ACTIVITY_OPCODE.HasFlag(opcode))
                    {
                        PlayerEndPoint playerEndPoint;
                        if (_playerEndPoints.TryGetValue(endPoint, out playerEndPoint))
                        {
                            playerEndPoint.InactivityEvent?.Reset();
                        }
                    }

                    Action<BinaryReader> readMethod;
                    if (_opcodeMapping.TryGetValue(opcode, out readMethod))
                    {
                        readMethod(br);
                    }
                    else
                    {
                        if (opcode == Opcodes.PING)
                        {
                            HandlePing(br, endPoint);
                        }
                        else
                        {
                            // Malformed packet or malformed read structures.
                            return;
                        }
                    }

                    // Should be quite a lot cheaper than throwing exceptions all the time.
                    if (br.BaseStream.Position == buffer.Length)
                    {
                        return;
                    }
                }
            }
            catch (EndOfStreamException)
            {
            }
            finally
            {
                br.Dispose();
            }
        }

        private void ReadPositionUpdate(BinaryReader reader)
        {
            int id = reader.ReadInt32();
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();

            reader.ReadByte(); // ASCII.EOT

            Map.UpdatePosition(new Point(x, y), id);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        public void WritePositionUpdate(List<GameObjectDAO> positions)
        {
            if (positions.Count == 0)
            {
                return;
            }
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((uint)Opcodes.BROADCAST_POSITION_UPDATE);
                bw.Write((byte)ASCII.STX);
                for (int i = 0; i < positions.Count; ++i)
                {
                    GameObjectDAO op = positions[i];
                    bw.Write(op.Id);
                    bw.Write(op.X);
                    bw.Write(op.Y);
                    if (i != positions.Count - 1)
                    {
                        bw.Write((byte)ASCII.US);
                    }
                }
                bw.Write((byte)ASCII.EOT);
                SendUdp(ms.ToArray());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        public void WriteCharacterStatUpdate(List<GameObjectDAO> characters)
        {
            if (characters.Count == 0)
            {
                return;
            }
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((uint)Opcodes.BROADCAST_CHARACTER_STAT_UPDATE);
                bw.Write((byte)ASCII.STX);

                for (int i = 0; i < characters.Count; i++)
                {
                    GameObjectDAO character = characters[i];
                    bw.Write(character.Id);
                    bw.Write(character.Score);
                    bw.Write(character.MaxHealth);
                    bw.Write(character.Health);
                    if (i != characters.Count - 1)
                    {
                        bw.Write((byte)ASCII.US);
                    }
                }

                bw.Write((byte)ASCII.EOT);

                SendUdp(ms.ToArray());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        public void WriteObjectDestruction(int objectId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((uint)Opcodes.OBJECT_DESTRUCTION);
                bw.Write((byte)ASCII.STX);
                bw.Write(objectId);

                bw.Write((byte)ASCII.EOT);
                BroadcastTcp(ms.ToArray());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        public void WriteCreateCharacter(string nickname, GameObjectDAO charData)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((uint)Opcodes.NEW_PLAYER);
                bw.Write((byte)ASCII.STX);
                bw.Write(nickname);
                bw.Write(charData.Id);
                bw.Write(charData.X);
                bw.Write(charData.Y);
                bw.Write(charData.Width);
                bw.Write(charData.Height);
                bw.Write(charData.Specialization);
                bw.Write(charData.Type);

                bw.Write((byte)ASCII.EOT);
                BroadcastTcp(ms.ToArray());
            }
        }

        public void WriteServerMessage(String message)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((uint)Opcodes.SERVER_MESSAGE);
                bw.Write((byte)ASCII.STX);
                bw.Write(message);

                bw.Write((byte)ASCII.EOT);
                BroadcastTcp(ms.ToArray());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        public void WriteRemoveCharacter(int playerId, int characterId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((uint)Opcodes.DISCONNECTED_PLAYER);
                bw.Write((byte)ASCII.STX);
                bw.Write(playerId);
                bw.Write(characterId);

                bw.Write((byte)ASCII.EOT);
                BroadcastTcp(ms.ToArray());
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        private void BulletCreationRequest(BinaryReader reader) // TODO split this in read and write.
        {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            double diameter = reader.ReadDouble();
            double velocityX = reader.ReadDouble();
            double velocityY = reader.ReadDouble();
            int bulletType = reader.ReadInt32();
            int damage = reader.ReadInt32();
            int ownerId = reader.ReadInt32();
            int entityType = reader.ReadInt32();

            reader.ReadByte(); // ASCII.EOT

            int bulletId = Map.AddBullet(x, y, velocityX, velocityY, diameter, damage, bulletType, ownerId);
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((uint)Opcodes.REQUEST_BULLET);
                bw.Write((byte)ASCII.STX);
                bw.Write(x);
                bw.Write(y);
                bw.Write(diameter);
                bw.Write(velocityX);
                bw.Write(velocityY);
                bw.Write(bulletType);
                bw.Write(damage);
                bw.Write(ownerId);
                bw.Write(bulletId);
                bw.Write(entityType);

                bw.Write((byte)ASCII.EOT);
                BroadcastTcp(ms.ToArray());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        public void KillNotification(int victimId, int killerId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((uint)Opcodes.KILL_NOTIFICATION);
                bw.Write((byte)ASCII.STX);
                bw.Write(victimId);
                bw.Write(killerId);

                bw.Write((byte)ASCII.EOT);
                BroadcastTcp(ms.ToArray());
            }
        }
    }
}