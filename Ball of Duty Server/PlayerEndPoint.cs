using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Utility;
using SocketExtensions;

namespace Ball_of_Duty_Server
{
    public class PlayerEndPoint
    {
        public int PlayerId { get; set; }

        public IPEndPoint UdpIpEndPoint { get; set; }

        public string SessionId { get; set; }

        public AsyncSocket TCPSocket { get; set; }

        public LightEvent InactivityEvent { get; set; }

        public PlayerEndPoint(int playerId, string sessionId)
        {
            PlayerId = playerId;
            SessionId = sessionId;
        }
    }
}