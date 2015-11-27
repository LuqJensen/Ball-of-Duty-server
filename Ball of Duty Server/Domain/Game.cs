using System;
using System.Collections.Generic;
using System.Linq;
using Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations;
using Ball_of_Duty_Server.Domain.Maps;
using Ball_of_Duty_Server.DTO;
using Ball_of_Duty_Server.Persistence;

namespace Ball_of_Duty_Server.Domain
{
    public class Game
    {
        public int Id { get; set; }

        public Map Map { get; set; } = new Map(4000, 4000);

        private Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public void AddPlayer(Player player, string clientIp, int clientUdpPort, int clientTcpPort,
            Specializations clientSpecialization)
        {
            _players.Add(player.Id, player);
            player.CurrentCharacter = Map.AddCharacter(player.Nickname, clientSpecialization);
            // TODO data to character creation should be dynamic

            Map.Broker.AddTarget(player.Id, clientIp, clientUdpPort, clientTcpPort);
        }

        public PlayerDTO[] ExportPlayers()
        {
            return _players.Values.Select(p => new PlayerDTO
            {
                Id = p.Id,
                Nickname = p.Nickname,
                CharacterId = p.CurrentCharacter?.Id ?? 0,
                Gold = p.Gold,
                HighScore = p.HighScore
                // TODO: look into some kind of assurance that CurrentCharacter is never null.
            }).ToArray();
        }

        public void RemovePlayer(int playerId)
        {
            Player player;
            if (_players.TryGetValue(playerId, out player))
            {
                player.CurrentCharacter?.Destroy();
                _players.Remove(playerId);
            }
        }

        public bool IsFull()
        {
            return _players.Count >= 10;
        }
    }
}