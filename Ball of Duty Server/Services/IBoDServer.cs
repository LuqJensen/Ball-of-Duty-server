using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain;
using Ball_of_Duty_Server.Persistence;

namespace Ball_of_Duty_Server.Services
{
    [ServiceContract]
    interface IBoDServer
    {
        [OperationContract]
        ServerPlayer newGuest();
     
        [OperationContract]
        ServerGameObject[] joinGame(ServerPlayer clientPlayer);
    }
}
