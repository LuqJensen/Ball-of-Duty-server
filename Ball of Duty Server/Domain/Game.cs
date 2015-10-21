using System;
using System.Collections.Generic;
using Ball_of_Duty_Server.Persistence;

namespace Ball_of_Duty_Server.Domain
{
    public class Game
    {
        private Dictionary<int, ServerPlayer> _players;

        public Game()
        {
            GameMap = new Map();

        }

        public int Id { get; set; }

        public Map GameMap { get; set; }

        public void AddPlayer(ServerPlayer serverPlayer)
        {
            _players.Add(serverPlayer.Id, serverPlayer);
        }

        public void RemovePlayer(ServerPlayer serverPlayer)
        {
            _players.Remove(serverPlayer.Id);
        }

        public bool isFull()
        {
            return false;
        }
    }
}