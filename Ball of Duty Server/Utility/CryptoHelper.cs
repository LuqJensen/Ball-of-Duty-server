using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Utility
{
    public static class CryptoHelper
    {
        public static byte[] FillRandomly(this byte[] buffer)
        {
            using (RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider())
            {
                csprng.GetBytes(buffer);
            }

            return buffer;
        }
    }
}