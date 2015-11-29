using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Persistence
{
    public class DataModelFacade
    {
        public static Player CreatePlayer(string nickname)
        {
            using (DatabaseContainer dc = new DatabaseContainer())
            {
                Player player = new Player() { Nickname = nickname }; // TODO: dynamic account.
                dc.Players.Add(player);
                dc.SaveChanges();
                return player;
            }
        }

        public static Account CreateAccount(string username, string nickname, int playerId, byte[] salt, byte[] hash)
        {
            using (DatabaseContainer dc = new DatabaseContainer())
            {
                if (dc.Accounts.FirstOrDefault(a => a.Username == username) != null)
                {
                    throw new ArgumentException($"Username: {username} is already taken");
                }

                Player player = dc.Players.FirstOrDefault(p => p.Id == playerId) ?? CreatePlayer(nickname);
                Account account = new Account()
                {
                    Username = username,
                    Player = player,
                    Salt = Encoding.ASCII.GetString(salt),
                    Hash = Encoding.ASCII.GetString(hash)
                };

                player.Account = account;

                dc.Accounts.Add(account);
                dc.SaveChanges();
                return account;
            }
        }


        public static IEnumerable<Player> GetHighestScoringPlayers()
        {
            using (DatabaseContainer dc = new DatabaseContainer())
            {
                return dc.Players.Select(p => p).OrderByDescending(p => p.HighScore).Take(100);
            }
        }
    }
}