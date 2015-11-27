using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.DTO
{
    [Serializable]
    public struct PlayerDTO
    {
        public int Id;
        public string Nickname;
        public int CharacterId;
        public double HighScore;
        public int Gold;
    }
}