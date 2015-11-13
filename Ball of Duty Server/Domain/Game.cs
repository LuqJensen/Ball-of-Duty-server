using System;
using System.Collections.Generic;
using Ball_of_Duty_Server.DAO;
using Ball_of_Duty_Server.Domain.Maps;
using Ball_of_Duty_Server.DTO;
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

            GameObjectDAO data = new GameObjectDAO
            {
                X = player.CurrentCharacter.Body.Position.X,
                Y = player.CurrentCharacter.Body.Position.Y,
                Width = player.CurrentCharacter.Body.Width,
                Height = player.CurrentCharacter.Body.Height,
                Id = player.CurrentCharacter.Id
            };

            Map.Broker.WriteCreateCharacter(player.Id, data, clientIp, clientPort);
        }

        public PlayerDTO[] ExportPlayers()
        {
            List<PlayerDTO> players = new List<PlayerDTO>();

            foreach (Player p in _players.Values)
            {
                players.Add(new PlayerDTO
                {
                    Id = p.Id,
                    Nickname = p.Nickname
                });
            }


            return players.ToArray();
        }

        public void RemovePlayer(int playerId)
        {
            Player player;
            if (_players.TryGetValue(playerId, out player))
            {
                _players.Remove(playerId);
                if (player.CurrentCharacter != null)
                {
                    Map.RemoveCharacter(player.CurrentCharacter.Id);
                }
            }
            Map.Broker.RemoveTarget(playerId);
        }

        public bool IsFull()
        {
            return _players.Count >= 10;
        }
    }
}