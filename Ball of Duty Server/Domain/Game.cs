﻿using System;
using System.Collections.Concurrent;
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
        private static volatile int _createdGames = 0;

        private ConcurrentDictionary<int, Player> _players = new ConcurrentDictionary<int, Player>();

        public int Id { get; } = ++_createdGames;

        public Map Map { get; } = new Map(3300, 3300);

        public void AddPlayer(Player player, Specializations clientSpecialization)
        {
            if (_players.TryAdd(player.Id, player))
            {
                player.CurrentCharacter = Map.AddCharacter(player.Nickname, clientSpecialization);
                // TODO data to character creation should be dynamic
            }
        }

        public PlayerDTO[] ExportPlayers()
        {
            return _players.Values.Select(p => new PlayerDTO
            {
                Id = p.Id,
                Nickname = p.Nickname,
                CharacterId = p.CurrentCharacter?.Id ?? 0,
                // TODO: look into some kind of assurance that CurrentCharacter is never null.
                Gold = p.Gold,
                HighScore = p.HighScore
            }).ToArray();
        }

        public GameObjectDTO Respawn(int playerId, Specializations clientSpecialization)
        {
            Player p;
            if (!_players.TryGetValue(playerId, out p))
            {
                return new GameObjectDTO();
            }
            p.CurrentCharacter = Map.AddCharacter(p.Nickname, clientSpecialization);
            Body b = p.CurrentCharacter.Body;

            return new GameObjectDTO()
            {
                Id = p.CurrentCharacter.Id,
                Body = new BodyDTO
                {
                    Position = new PointDTO
                    {
                        X = b.Position.X,
                        Y = b.Position.Y
                    },
                    Width = b.Width,
                    Height = b.Height,
                    Type = (int)b.Type
                }
            };
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