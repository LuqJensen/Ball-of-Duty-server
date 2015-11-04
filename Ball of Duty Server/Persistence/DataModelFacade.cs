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
                Player p = new Player() { Nickname = nickname }; // TODO: dynamic account.
                dc.Players.Add(p);
                dc.SaveChanges();
                return p;
            }
        }
    }
}