using System.Collections.Generic;

namespace Ball_of_Duty_Server.Domain
{
    public class BoDServer
    {
        

        public BoDServer()
        {
            Games = new List<Game>();
            
        }

        public List<Game> Games
        {
            get; set;
        }

        public Game CreateGame()
        {
            return null;
        }

        public void CreatePlayer()
        {
            throw new System.NotImplementedException();
        }
    }
}