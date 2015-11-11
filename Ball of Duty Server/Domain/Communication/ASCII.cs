using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Domain.Communication
{
    public enum ASCII : byte
    {
        SOH = 1, // Start of heading.
        STX = 2, // Start of text.
        EOT = 4, // End of transmission.
        US = 31 // Unit separator.
    }
}