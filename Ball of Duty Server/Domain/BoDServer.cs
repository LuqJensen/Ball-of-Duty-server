using System;
using System.Collections.Generic;
using System.Linq;
using Ball_of_Duty_Server.Persistence;
using Ball_of_Duty_Server.Services;

namespace Ball_of_Duty_Server.Domain
{
    public class BoDServer : IBoDServer
    {
        public BoDServer()
        {
            if (Games == null)
            {
                Games = new List<Game>();
            }
           
        }

        public static List<Game> Games { get; set; }

        public ServerPlayer newGuest()
        {
            return new ServerPlayer();
        }

        public Game getGame()
        {
            if (!Games.Any())
            {
                return new Game();
            }
            foreach (var Game in Games)
            {
                if (!Game.isFull())
                {
                    return Game; 
                }
            }

            return new Game();
        }

        public List<ServerGameObject> joinGame(ServerPlayer clientPlayer)
        {
            Console.Write("id: "+clientPlayer.Id+" Tried to join game.");
            Game returnedGame = getGame();
            Games.Add(returnedGame);
            Map gameMap= returnedGame.GameMap;

            // gameMap.GameObjects.TryAdd(clientPlayer.Id, new Character(clientPlayer.Id)); 
            // Client cant handle it being a list for some reason. For now the return is null
            return gameMap.GetObjects();

        }


        public Game CreateGame()
        {
            return null;
        }
    }
}