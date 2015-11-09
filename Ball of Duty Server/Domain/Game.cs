using System;
using System.Collections.Generic;
using Ball_of_Duty_Server.DAO;
using Ball_of_Duty_Server.Persistence;

namespace Ball_of_Duty_Server.Domain
{
    public class Game
    {
        public int Id { get; set; }

        public Map Map { get; set; } = new Map();

        private Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public void AddPlayer(Player player, string clientIp, int clientPort)
        {
            _players.Add(player.Id, player);
            player.CurrentCharacter = Map.AddCharacter(); // TODO data to character creation should be dynamic

            GameObjectDAO data = new GameObjectDAO();
            data.X = player.CurrentCharacter.Body.Position.X;
            data.Y = player.CurrentCharacter.Body.Position.Y;
            data.Width = player.CurrentCharacter.Body.Width;
            data.Height = player.CurrentCharacter.Body.Height;
            data.Id = player.CurrentCharacter.Id;

            Map.Broker.AddPlayer(player.Id, data, clientIp,
                clientPort);
        }

        public void RemovePlayer(int playerId)
        {
            Player player;
            if (_players.TryGetValue(playerId, out player))
            {
                _players.Remove(playerId);
                Map.RemoveCharacter(player.CurrentCharacter.Id);
            }
            Map.Broker.RemoveTarget(playerId);
        }

        public bool IsFull()
        {
            return _players.Count >= 10;
        }
    }
}