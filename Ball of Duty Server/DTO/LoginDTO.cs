using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.DTO
{
    [Serializable]
    public class LoginDTO
    {
        public byte[] SessionSalt;
        public byte[] PasswordSalt;
        public byte[] AuthenticationChallenge;
        public byte[] IV;
    }
}