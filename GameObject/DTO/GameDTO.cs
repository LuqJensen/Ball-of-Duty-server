using System;

namespace GameObject.DTO
{
    [Serializable]
    public class GameDTO
    {
        public byte[] SessionId;
        public GameObjectDTO[] GameObjects;
        public PlayerDTO[] Players;
        public int GameId;
        public int CharacterId;
        public int MapWidth;
        public int MapHeight;
        public string IpAddress;
        public int UdpPort;
        public int TcpPort;
        public string Version;
    }
}