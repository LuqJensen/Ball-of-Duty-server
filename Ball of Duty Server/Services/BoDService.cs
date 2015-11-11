using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
using Ball_of_Duty_Server.Domain;
using Ball_of_Duty_Server.Domain.Maps;
using Ball_of_Duty_Server.DTO;
using Ball_of_Duty_Server.Persistence;

namespace Ball_of_Duty_Server.Services
{
    public class BoDService : IBoDService
    {
        public BoDService()
        {
        }

        public static Dictionary<int, Game> Games { get; set; } = new Dictionary<int, Game>();

        public static Dictionary<int, Game> PlayerIngame { get; set; } = new Dictionary<int, Game>(); // midlertidig

        public static Dictionary<int, Player> OnlinePlayers { get; set; } = new Dictionary<int, Player>();

        /*private static string localIPAddress;

        public static string LocalIPAddress
        {
            get
            {
                if (localIPAddress == null)
                {
                    //  http://stackoverflow.com/a/27376368
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("10.0.2.4", 65530);
                        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                        localIPAddress = endPoint?.Address.ToString();
                    }
                }
                return localIPAddress;
            }
        }*/

        public PlayerDTO NewGuest(string nickname)
        {
            Player p = DataModelFacade.CreatePlayer(nickname);
            OnlinePlayers.Add(p.Id, p);

            return new PlayerDTO { Id = p.Id, Nickname = p.Nickname };
        }

        private Game GetGame()
        {
            foreach (Game g in Games.Values)
            {
                if (!g.IsFull())
                {
                    return g;
                }
            }

            Game game = new Game();
            Console.WriteLine("Newly created game id: " + game.Id);
            Games.Add(game.Id, game);
            return game;
        }

        public GameDTO JoinGame(int clientPlayerId, int clientPort)
        {
            Player player;


            if (!OnlinePlayers.TryGetValue(clientPlayerId, out player))
            {
                return new GameDTO(); //TODO: probably not the smartest, but necessary.
            }

            Console.WriteLine($"Player: {clientPlayerId} tried to join game.");

            Game game = GetGame();
            Map map = game.Map;

            #region GetClientIp

            OperationContext context = OperationContext.Current;
            MessageProperties properties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint =
                properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            string clientIp = endpoint?.Address;

            if (clientIp == null) //TODO: probably not the smartest, but necessary.
            {
                return new GameDTO();
            }

            #endregion

            game.AddPlayer(player, clientIp, clientPort);
            if (!PlayerIngame.ContainsKey(player.Id)) //TODO: brug OnlinePlayers istedet
            {
                PlayerIngame.Add(player.Id, game);
            }

            Console.WriteLine($"Current gameobjects: {map.GameObjects.Count}");

            return new GameDTO
            {
                GameObjects = map.ExportGameObjects(),
                CharacterId = player.CurrentCharacter.Id,
                GameId = game.Id
            };
            //TODO: Add servers IP here -- maybe rename to GameDTO?
        }

        public void QuitGame(int clientPlayerId /*, int gameId*/) //TODO: brug OnlinePlayers istedet
        {
            // Removes the player from the game
            Game game;
            if (PlayerIngame.TryGetValue(clientPlayerId, out game))
                //if (Games.TryGetValue(gameId, out game)) //TODO: brug OnlinePlayers i stedet
            {
                PlayerIngame.Remove(clientPlayerId); //TODO: brug OnlinePlayers istedet
                game.RemovePlayer(clientPlayerId);
                Console.WriteLine($"Player: {clientPlayerId} quit game: {game.Id}.");
            }
            else
            {
                Debug.WriteLine($"Player: {clientPlayerId} failed quitting game" /*: {gameId}."*/);
            }
        }
    }
}