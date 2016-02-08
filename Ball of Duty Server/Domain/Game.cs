using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Ball_of_Duty_Server.Domain.Maps;
using Ball_of_Duty_Server.DTO;
using Ball_of_Duty_Server.Persistence;
using Entity.DTO;
using Entity.Entities;

namespace Ball_of_Duty_Server.Domain
{
    public class Game
    {
        public const int MAP_SIZE = 3300;
        private static int _createdGames = 0;

        private ConcurrentDictionary<int, Player> _players = new ConcurrentDictionary<int, Player>();

        public int Id { get; } = Interlocked.Increment(ref _createdGames);

        public Map Map { get; }

        public Game()
        {
            Map = new Map(Id, MAP_SIZE, MAP_SIZE);
        }

        public void AddPlayer(Player player, int clientSpecialization)
        {
            if (_players.TryAdd(player.Id, player))
            {
                player.CurrentCharacter = Map.AddCharacter(player.Nickname, clientSpecialization);
            }
        }

        public void WriteServerMessage(string message)
        {
            Map.Broker.WriteServerMessage(message);
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
            }).ToArray();
        }

        public GameObjectDTO Respawn(int playerId, int clientSpecialization)
        {
            Player p;
            if (!_players.TryGetValue(playerId, out p))
            {
                return new GameObjectDTO();
            }

            ICharacter character = Map.AddCharacter(p.Nickname, clientSpecialization);
            p.CurrentCharacter = character;

            return character.Export();
        }


        public void RemovePlayer(int playerId)
        {
            Player player;
            if (_players.TryRemove(playerId, out player))
            {
                player.CurrentCharacter?.Destroy();
            }
        }

        public bool IsFull()
        {
            return false;
        }
    }
}