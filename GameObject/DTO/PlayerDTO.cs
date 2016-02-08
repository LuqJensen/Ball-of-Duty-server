using System;

namespace GameObject.DTO
{
    [Serializable]
    public class PlayerDTO
    {
        public int Id;
        public string Nickname;
        public int CharacterId;
        public double HighScore;
        public int Gold;
    }
}