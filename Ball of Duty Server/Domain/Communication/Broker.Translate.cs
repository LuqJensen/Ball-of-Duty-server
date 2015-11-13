using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ball_of_Duty_Server.DAO;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.DTO;

namespace Ball_of_Duty_Server.Domain.Communication
{
    public partial class Broker
    {
        private readonly Dictionary<Opcodes, Action<BinaryReader>> _opcodeMapping =
            new Dictionary<Opcodes, Action<BinaryReader>>();

        private void AddOpcodeMapping()
        {
            _opcodeMapping.Add(Opcodes.POSITION_UPDATE, this.ReadPositionUpdate);
            _opcodeMapping.Add(Opcodes.REQUEST_BULLET, this.BulletCreationRequest);
        }

        private void Read(byte[] buffer)
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

        private void ReadPositionUpdate(BinaryReader reader)
        {
            do
            {
                int id = reader.ReadInt32();
                double x = reader.ReadDouble();
                double y = reader.ReadDouble();

                Map.UpdatePosition(new Point(x, y), id); // TODO preferably call this method once per positionupdate.
            } while (reader.ReadByte() == (byte)ASCII.US);
        }

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
                bw.Write((byte)Opcodes.BROADCAST_POSITION_UPDATE);
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

        public void WriteScoreUpdate(List<GameObjectDAO> characters)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((byte)Opcodes.BROADCAST_SCORE_UPDATE);
                bw.Write((byte)ASCII.STX);
                for (int i = 0; i < characters.Count; i++)
                {
                    GameObjectDAO character = characters[i];
                    bw.Write(character.Id);
                    bw.Write(character.Score);
                    if (i != characters.Count - 1)
                    {
                        bw.Write((byte)ASCII.US);
                    }
                }

                bw.Write((byte)ASCII.EOT);

                SendUdp(ms.ToArray());
            }
        }


        public void WriteHealthUpdate(List<GameObjectDAO> characters)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((byte)Opcodes.BROADCAST_HEALTH_UPDATE);
                bw.Write((byte)ASCII.STX);
                for (int i = 0; i < characters.Count; i++)
                {
                    GameObjectDAO character = characters[i];
                    bw.Write(character.Id);
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

        public void WriteObjectDestruction(int objectId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((byte)Opcodes.OBJECT_DESTRUCTION);
                bw.Write((byte)ASCII.STX);
                bw.Write(objectId);

                bw.Write((byte)ASCII.EOT);
                SendTcp(ms.ToArray());
            }
        }

        public void WriteCreateCharacter(int playerId, GameObjectDAO charData, string ip, int preferedPort)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((byte)Opcodes.NEW_PLAYER);
                bw.Write((byte)ASCII.STX);
                bw.Write(playerId);
                bw.Write(charData.Id);
                bw.Write(charData.X);
                bw.Write(charData.Y);
                bw.Write(charData.Width);
                bw.Write(charData.Height);
                bw.Write(0); // Temporary till EntityType enum is implemented on server.

                bw.Write((byte)ASCII.EOT);
                SendTcp(ms.ToArray());
            }

            AddTarget(playerId, ip, preferedPort);
        }

        public void WriteRemoveCharacter(int playerId, int characterId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((byte)Opcodes.DISCONNECTED_PLAYER);
                bw.Write((byte)ASCII.STX);
                bw.Write(playerId);
                bw.Write(characterId);

                bw.Write((byte)ASCII.EOT);
                SendTcp(ms.ToArray());
            }

            RemoveTarget(playerId);
        }

        private void BulletCreationRequest(BinaryReader reader) // TODO split this in read and write.
        {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            double radius = reader.ReadDouble();
            double velocityX = reader.ReadDouble();
            double velocityY = reader.ReadDouble();
            int bulletType = reader.ReadInt32();
            int damage = reader.ReadInt32();
            int ownerId = reader.ReadInt32();
            int entityType = reader.ReadInt32();

            int bulletId = Map.AddBullet(x, y, velocityX, velocityY, radius, damage, ownerId);
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((byte)Opcodes.REQUEST_BULLET);
                bw.Write((byte)ASCII.STX);
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

                bw.Write((byte)ASCII.EOT);
                SendTcp(ms.ToArray());
            }
        }

        public void KillNotification(int victimId, int killerId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ASCII.SOH);
                bw.Write((byte)Opcodes.KILL_NOTIFICATION);
                bw.Write((byte)ASCII.STX);
                bw.Write(victimId);
                bw.Write(killerId);

                bw.Write((byte)ASCII.EOT);
                SendTcp(ms.ToArray());
            }
        }
    }
}