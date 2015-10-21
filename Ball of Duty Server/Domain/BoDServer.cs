using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        public ServerGameObject[] joinGame(ServerPlayer clientPlayer)
        {
           
            Console.WriteLine("id: "+clientPlayer.Id+" Tried to join game.");
            Game returnedGame = getGame();
            Games.Add(returnedGame);
            Map gameMap= returnedGame.GameMap;

             gameMap.GameObjects.TryAdd(clientPlayer.Id, new ServerCharacter(clientPlayer.Id));


            Console.WriteLine("count: "+gameMap.GameObjects.Count);

            ServerGameObject[] tempArray = gameMap.GameObjects.Values.ToArray();
            ServerGameObject[] returnedValue = new ServerGameObject[gameMap.GameObjects.Values.ToArray().Length];
            for (int i = 0; i< returnedValue.Length;i++)
            {
                returnedValue[i] = new ServerGameObject(tempArray[i].getID());

            }

            return returnedValue;

        }


        public Game CreateGame()
        {
            return null;
        }
    }
}