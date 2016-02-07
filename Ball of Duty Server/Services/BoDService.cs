using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
using Ball_of_Duty_Server.Domain;
using Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations;
using Ball_of_Duty_Server.Domain.GameObjects.Components;
using Ball_of_Duty_Server.Domain.Maps;
using Ball_of_Duty_Server.DTO;
using Ball_of_Duty_Server.Exceptions;
using Ball_of_Duty_Server.Persistence;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Services
{
    public class BoDService : IBoDService
    {
        public const string VERSION = "0.0.1";
        public static ConcurrentDictionary<int, Game> Games { get; } = new ConcurrentDictionary<int, Game>();

        public static ConcurrentDictionary<int, Game> PlayerIngame { get; } = new ConcurrentDictionary<int, Game>(); // midlertidig

        public static ConcurrentDictionary<int, Player> OnlinePlayers { get; } = new ConcurrentDictionary<int, Player>();

        public static ConcurrentDictionary<string, Account> AccountsAuthenticating { get; } = new ConcurrentDictionary<string, Account>();

        private static string IpAddress { get; } = ConfigurationManager.AppSettings["serverIp"];

        static BoDService()
        {
            GetGame();
        }


        public PlayerDTO NewGuest(string nickname)
        {
            Player p = DataModelFacade.CreatePlayer(nickname);
            OnlinePlayers.TryAdd(p.Id, p);

            return new PlayerDTO
            {
                Id = p.Id,
                Nickname = p.Nickname
            };
        }

        public static void WriteServerMessage(string message)
        {
            foreach (Game game in Games.Values)
            {
                game.WriteServerMessage(message);
            }
        }

        public AccountDTO NewAccount(string username, string nickname, int playerId, byte[] salt, byte[] hash)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(nickname) || salt.Length != CryptoHelper.SALT_SIZE ||
                hash.Length != CryptoHelper.HASH_SIZE)
            {
                return new AccountDTO();
            }
            try
            {
                Account a = DataModelFacade.CreateAccount(username, nickname, playerId, salt, hash);
                return new AccountDTO
                {
                    Id = a.Id,
                    Player = new PlayerDTO
                    {
                        Id = a.Player.Id,
                        Nickname = a.Player.Nickname
                    }
                };
            }
            catch (ArgumentException)
            {
                return new AccountDTO();
            }
        }

        private static Game GetGame()
        {
            foreach (Game g in Games.Values)
            {
                if (!g.IsFull())
                {
                    return g;
                }
            }

            Game game = new Game();
            BoDConsole.WriteLine($"Created new game: {game.Id}");
            Games.TryAdd(game.Id, game);
            return game;
        }

        public GameObjectDTO Respawn(int clientPlayerId, int clientSpecializations)
        {
            Game game;
            if (!PlayerIngame.TryGetValue(clientPlayerId, out game))
            {
                BoDConsole.WriteLine("Coulnd't find player");
                return new GameObjectDTO();
            }
            BoDConsole.WriteLine($"Player: {clientPlayerId} tried to respawn.");

            return game.Respawn(clientPlayerId, clientSpecializations);
        }

        public GameDTO JoinGame(int clientPlayerId, int clientSpecialization, string clientVersion)
        {
            if (!clientVersion.Equals(VERSION)) //TODO: Should maybe allow more than just the latest update later on.
            {
                throw new FaultException<VersionOutdatedFault>(new VersionOutdatedFault
                {
                    Message = $"Required version is {VERSION}, but the version found was {clientVersion}"
                });
            }
            Player player;
            if (!OnlinePlayers.TryGetValue(clientPlayerId, out player))
            {
                return new GameDTO(); //TODO: probably not the smartest, but necessary.
            }


            Game game = GetGame();
            Map map = game.Map;

            /* #region GetClientIp

            OperationContext context = OperationContext.Current;
            MessageProperties properties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            string clientIp = endpoint?.Address;

            if (clientIp == null)
            {
                throw new FaultException("There were issues establishing a connection, please try again later.");
            }

            #endregion*/

            game.AddPlayer(player, clientSpecialization);
            if (!PlayerIngame.TryAdd(player.Id, game)) //TODO: brug OnlinePlayers istedet
            {
                Console.WriteLine("shit nigga");
            }

            return new GameDTO
            {
                SessionId = map.Broker.GenerateSessionId(player.Id),
                Players = game.ExportPlayers(),
                GameObjects = map.ExportGameObjects(),
                CharacterId = player.CurrentCharacter.Id,
                GameId = game.Id,
                MapWidth = map.Width,
                MapHeight = map.Height,
                IpAddress = IpAddress,
                TcpPort = map.Broker.TcpPort,
                UdpPort = map.Broker.UdpPort,
                Version = VERSION
            };
        }

        public void QuitGame(int clientPlayerId /*, int gameId*/) //TODO: brug OnlinePlayers istedet
        {
            // Removes the player from the game
            Game game;
            if (PlayerIngame.TryRemove(clientPlayerId, out game))
                //if (Games.TryGetValue(gameId, out game)) //TODO: brug OnlinePlayers i stedet
            {
                game.RemovePlayer(clientPlayerId);
            }
            else
            {
                Debug.WriteLine($"Player: {clientPlayerId} failed quitting game" /*: {gameId}."*/);
            }
        }

        public LoginDTO RequestAuthenticationChallenge(string username)
        {
            try
            {
                Account account = DataModelFacade.GetAccount(a => a.Username == username);
                account.SessionSalt = CryptoHelper.GenerateSalt();
                account.CryptoHelper = new CryptoHelper(account.Hash, account.SessionSalt);
                account.AuthenticationChallenge = CryptoHelper.GenerateSalt();

                AccountsAuthenticating[account.Username] = account;

                return new LoginDTO
                {
                    PasswordSalt = Convert.FromBase64String(account.Salt),
                    SessionSalt = account.SessionSalt,
                    IV = account.CryptoHelper.GenerateIV(),
                    AuthenticationChallenge = account.CryptoHelper.Encrypt(account.AuthenticationChallenge)
                };
            }
            catch (InvalidOperationException)
            {
                throw new FaultException<InvalidAuthenticationFault>(new InvalidAuthenticationFault
                {
                    Message = "Invalid username."
                });
            }
        }

        public PlayerDTO CompleteAuthenticationChallenge(string username, byte[] decryptedChallenge)
        {
            Account account;
            if (AccountsAuthenticating.TryRemove(username, out account) &&
                Convert.ToBase64String(account.AuthenticationChallenge) == Convert.ToBase64String(decryptedChallenge))
            {
                Player p = account.Player;
                if (OnlinePlayers.TryAdd(p.Id, p))
                {
                    return new PlayerDTO
                    {
                        Id = p.Id,
                        Gold = p.Gold,
                        HighScore = p.HighScore,
                        Nickname = p.Nickname
                    };
                }
            }
            throw new FaultException<InvalidAuthenticationFault>(new InvalidAuthenticationFault
            {
                Message = "Invalid password."
            });
        }

        public PlayerDTO[] GetLeaderboard()
        {
            return (from p in DataModelFacade.GetHighestScoringPlayers()
                select new PlayerDTO
                {
                    Id = p.Id,
                    Nickname = p.Nickname,
                    HighScore = p.HighScore
                }).ToArray();
        }
    }
}