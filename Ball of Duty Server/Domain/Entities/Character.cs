using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Character : GameObject
    {
        private double _score { get; set; }
        public int HP { get; set; } // PLACEHOLDER FOR THE HP CLASS

        private double _scoreUP = 100;
        private double _scoreFactor = 0.1;

        public int _killCount;
        

        public Character()
        {
            Body = new Body(this, new Point(0, 0), 50, 50) { Type = Body.Geometry.CIRCLE }; // TODO should be dynamic
            _score = 0;
            _killCount = 0;
            HP = 100;
        }
        /*
        Called when character gets a kill
        Increment killCount
        Increment score with _scoreUP and _scoreFactor percent of the victims score
            Notifies its observers (only player should react) 
        */
        public void AddKill(Character victim)
        {
            _killCount++;
            _score = _score + _scoreUP + (victim._score*_scoreFactor);
            NotifyObservers(victim);
        }

    }
}