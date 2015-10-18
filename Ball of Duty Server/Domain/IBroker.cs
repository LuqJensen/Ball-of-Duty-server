
using Ball_of_Duty_Server.DTO;
using System.Collections.Generic;

namespace Ball_of_Duty_Server.Domain
{
    public interface IBroker
    {
        void SendPositionUpdate(List<ObjectPosition> positions, int gameId);
    }
}