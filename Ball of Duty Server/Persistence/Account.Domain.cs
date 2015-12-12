using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Persistence
{
    public partial class Account
    {
        public byte[] AuthenticationChallenge { get; set; }

        public byte[] SessionSalt { get; set; }

        public CryptoHelper CryptoHelper { get; set; }
    }
}