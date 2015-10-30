using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
using Ball_of_Duty_Server.DTO;
using Ball_of_Duty_Server.Persistence;
using Ball_of_Duty_Server.Services;

namespace Ball_of_Duty_Server.Domain
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

        public PlayerDTO NewGuest()
        {
            Player p = new Player();
            OnlinePlayers.Add(p.Id, p);

            return new PlayerDTO { Id = p.Id, Nickname = p.Nickname };
        }

        public Game GetGame()
        {
            foreach (Game g in Games.Values)
            {
                if (!g.IsFull())
                {
                    return g; 
                }
            }

            Game game = new Game();
            Games.Add(game.Id, game);
            return game;
        }

        public MapDTO JoinGame(int clientPlayerId, int clientPort)
        {
            Player player;
            Console.WriteLine(clientPlayerId);
            if (!OnlinePlayers.TryGetValue(clientPlayerId, out player))
            {
                return new MapDTO(); // probably not the smartest, but necessary.
            }

            Console.WriteLine("id: "+ clientPlayerId + " Tried to join game.");
            Game game = GetGame();
            Map map = game.GameMap;

            #region GetClientIp
            OperationContext context = OperationContext.Current;
            MessageProperties properties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            string clientIp = endpoint?.Address;

            if (clientIp == null) // we cant operate with a null ip...
            {
                return new MapDTO();
            }
            #endregion

            game.AddPlayer(player, clientIp, clientPort);
            if (!PlayerIngame.ContainsKey(player.Id)) // midlertidig..
            {
                PlayerIngame.Add(player.Id, game);
            }

            map.GameObjects.TryAdd(clientPlayerId, new Character(clientPlayerId)); // det her burde ikke ligge her.

            Console.WriteLine("count: "+map.GameObjects.Count);
            Console.WriteLine("count: " + map.GameObjects.Values.Count);

            List<GameObjectDTO> gameObjects = new List<GameObjectDTO>();

            foreach (GameObject go in map.GameObjects.Values) // det her burde ikke ligge her...
            {
                PointDTO point = new PointDTO {X = go.Body.Position.X, Y = go.Body.Position.Y};
                BodyDTO body = new BodyDTO {Point = point};
                gameObjects.Add(new GameObjectDTO {Id = go.Id, Body = body});
            }

            return new MapDTO {GameObjects = gameObjects.ToArray()}; // det her virker kun mens vi tester lokalt!!!
        }

        public void QuitGame(int clientPlayerId/*, int gameId*/) // synes gameId skal komme fra clienten.
        {
            
            // Removes the player from the game
            Game game;
            if (PlayerIngame.TryGetValue(clientPlayerId, out game))
            //if (Games.TryGetValue(gameId, out game))
            {
                PlayerIngame.Remove(clientPlayerId); // midlertidig...
                game.RemovePlayer(clientPlayerId);
                Console.WriteLine($"Player: {clientPlayerId} quit game: {game.Id}.");

                // Removes the player character from the map - bør ikke ligge her, synes ikke Service skal kende til GameObject...
                GameObject go;
                game.GameMap.GameObjects.TryRemove(clientPlayerId, out go);
            }
            else
            {
                Debug.WriteLine($"Player: {clientPlayerId} failed quitting game"/*: {gameId}."*/);
            }
        }
    }
}