using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ball_of_Duty_Server.Persistence;

namespace Ball_of_Duty_Server.Domain
{
    public class Game
    {
        private System.Collections.Generic.Dictionary<int, Ball_of_Duty_Server.Persistence.Player> _players;

        public Game()
        {
            GameMap = new Map();
        }

        public int Id
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
            }
        }

        public Map GameMap
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
            }
        }

        public void AddPlayer(Player player)
        {
            _players.Add(player.Id, player);
        }

        public void RemovePlayer(Player player)
        {
            _players.Remove(player.Id);
        }
    }
}