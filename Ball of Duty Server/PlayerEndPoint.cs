using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server
{
    public class PlayerEndPoint
    {
        public int PlayerId { get; set; }
        public IPEndPoint IpEndPoint { get; set; }

        public PlayerEndPoint(IPEndPoint ipEndPoint, int playerId)
        {
            IpEndPoint = ipEndPoint;
            PlayerId = playerId;
        }
    }
}