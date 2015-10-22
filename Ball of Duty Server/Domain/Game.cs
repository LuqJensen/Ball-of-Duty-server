using System;
using System.Collections.Generic;
using Ball_of_Duty_Server.Persistence;

namespace Ball_of_Duty_Server.Domain
{
    public class Game
    {
        public int Id { get; set; }

        public Map GameMap { get; set; }

        private Dictionary<int, Player> _players;

        public Game(string ip)
        {
            GameMap = new Map(ip);
            _players = new Dictionary<int, Player>();
        }

        public void AddPlayer(Player player)
        {
            _players.Add(player.Id, player);
        }

        public void RemovePlayer(int playerID)
        {
            _players.Remove(playerID);
        }

        public bool IsFull()
        {
            return false;
        }
    }
}