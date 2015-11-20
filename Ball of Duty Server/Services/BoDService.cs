using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
using Ball_of_Duty_Server.Domain;
using Ball_of_Duty_Server.Domain.Communication;
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

        public static ConcurrentDictionary<int, Game> Games { get; set; } = new ConcurrentDictionary<int, Game>();

        public static ConcurrentDictionary<int, Game> PlayerIngame { get; set; } = new ConcurrentDictionary<int, Game>()
            ; // midlertidig

        public static ConcurrentDictionary<int, Player> OnlinePlayers { get; set; } =
            new ConcurrentDictionary<int, Player>();

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
            OnlinePlayers.TryAdd(p.Id, p);

            return new PlayerDTO { Id = p.Id, Nickname = p.Nickname };
        }

        public AccountDTO NewAccount(string username, string nickname, int playerId, byte[] salt, byte[] hash)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(nickname) || salt.Length != 32 ||
                hash.Length != 32)
            {
                return new AccountDTO();
            }
            try
            {
                Account a = DataModelFacade.CreateAccount(username, nickname, playerId, salt, hash);
                return new AccountDTO()
                {
                    Id = a.Id,
                    Player = new PlayerDTO() { Id = a.Player.Id, Nickname = a.Player.Nickname }
                };
            }
            catch
            {
                return new AccountDTO();
            }
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
            Games.TryAdd(game.Id, game);
            return game;
        }

        public GameDTO JoinGame(int clientPlayerId, int clientPort, int clientSpecialization)
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

            Specializations spec = (Specializations)clientSpecialization;
            game.AddPlayer(player, clientIp, clientPort, spec);
            if (!PlayerIngame.ContainsKey(player.Id)) //TODO: brug OnlinePlayers istedet
            {
                PlayerIngame.TryAdd(player.Id, game);
            }

            Console.WriteLine($"Current gameobjects: {map.GameObjects.Count}");

            return new GameDTO
            {
                Players = game.ExportPlayers(),
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
            if (PlayerIngame.TryRemove(clientPlayerId, out game))
                //if (Games.TryGetValue(gameId, out game)) //TODO: brug OnlinePlayers i stedet
            {
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