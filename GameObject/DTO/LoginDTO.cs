using System;

namespace GameObject.DTO
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