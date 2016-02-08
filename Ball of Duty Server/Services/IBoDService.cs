using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain;
using Ball_of_Duty_Server.DTO;
using Ball_of_Duty_Server.Exceptions;
using Ball_of_Duty_Server.Persistence;
using Entity.DTO;

namespace Ball_of_Duty_Server.Services
{
    [ServiceContract]
    public interface IBoDService
    {
        [OperationContract]
        PlayerDTO NewGuest(string nickname);

        [OperationContract]
        [FaultContract(typeof (VersionOutdatedFault))]
        GameDTO JoinGame(int clientPlayerId, int clientSpecialization, string clientVersion);

        [OperationContract]
        AccountDTO NewAccount(string username, string nickname, int playerId, byte[] salt, byte[] hash);

        [OperationContract]
        void QuitGame(int clientPlayerId);

        [OperationContract]
        PlayerDTO[] GetLeaderboard();

        [OperationContract]
        GameObjectDTO Respawn(int clientPlayerId, int clientSpecializations);

        [OperationContract]
        LoginDTO RequestAuthenticationChallenge(string username);

        [OperationContract]
        PlayerDTO CompleteAuthenticationChallenge(string username, byte[] decryptedChallenge);
    }
}