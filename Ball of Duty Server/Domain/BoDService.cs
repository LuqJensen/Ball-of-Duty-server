using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            if (Games == null)
            {
                Games = new List<Game>();
            }
           
        }

        public static List<Game> Games { get; set; }

        private static string localIPAddress;

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
        }

        public PlayerDTO NewGuest()
        {
            Player p = Player.CreatePlayer();
            
            return new PlayerDTO { Id = p.Id, Nickname = p.Nickname };
        }

        public Game GetGame()
        {
            foreach (var g in Games)
            {
                if (!g.IsFull())
                {
                    return g; 
                }
            }

            Game game = new Game(LocalIPAddress);
            Games.Add(game);
            return game;
        }

        public MapDTO JoinGame(int clientPlayerId)
        {
           
            Console.WriteLine("id: "+ clientPlayerId + " Tried to join game.");
            Game returnedGame = GetGame();
            Map gameMap= returnedGame.GameMap;

            gameMap.GameObjects.TryAdd(clientPlayerId, new Character(clientPlayerId));


            Console.WriteLine("count: "+gameMap.GameObjects.Count);
            
            List<GameObjectDTO> gameObjects = new List<GameObjectDTO>();

            foreach (GameObject go in gameMap.GameObjects.Values)
            {
                PointDTO point = new PointDTO {X = go.Body.Position.X, Y = go.Body.Position.Y};
                BodyDTO body = new BodyDTO {Point = point};
                gameObjects.Add(new GameObjectDTO {Id = go.Id, Body = body});
            }

            MapDTO map = new MapDTO {GameObjects = gameObjects.ToArray(), IPAddress = LocalIPAddress};

            return map;

        }


        public Game CreateGame()
        {
            return null;
        }
    }
}