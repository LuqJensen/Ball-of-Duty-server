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

        public Game()
        {
            GameMap = new Map();
            _players = new Dictionary<int, Player>();
        }

        public void AddPlayer(Player player, string clientIp, int clientPort)
        {
            _players.Add(player.Id, player);
            GameMap.Broker.AddTarget(player.Id, clientIp, clientPort);
        }

        public void RemovePlayer(int playerId)
        {
            _players.Remove(playerId);
            GameMap.Broker.RemoveTarget(playerId);
        }

        public bool IsFull()
        {
            return false;
        }
    }
}