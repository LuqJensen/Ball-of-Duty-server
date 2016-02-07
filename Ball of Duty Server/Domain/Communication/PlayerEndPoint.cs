using System.Net;
using AsyncSocket;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Domain.Communication
{
    public class PlayerEndPoint
    {
        public int PlayerId { get; set; }

        public IPEndPoint UdpIpEndPoint { get; set; }

        public string SessionId { get; set; }

        public IAsyncSocket TCPSocket { get; set; }

        public LightEvent InactivityEvent { get; set; }

        public PlayerEndPoint(int playerId, string sessionId)
        {
            PlayerId = playerId;
            SessionId = sessionId;
        }
    }
}