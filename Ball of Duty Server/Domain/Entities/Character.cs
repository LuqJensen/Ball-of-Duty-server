using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Character : GameObject
    {
        private double _score { get; set; }
        public const int MAX_HP = 107;

        private double _scoreUP = 100;
        private double _scoreUPFactor = 0.1;
        private double _scoreDecayFactor = 0.01;
        private double _allowedScoreBeforeDecay = 400;

        public int _killCount;


        public Character()
        {
            Body = new Body(this, new Point(0, 0), 50, 50) { Type = Body.Geometry.CIRCLE }; // TODO should be dynamic
            _score = 0;
            _killCount = 0;
            Health = new Health(this, MAX_HP);
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
            _score += _scoreUP + (victim._score * _scoreUPFactor);
            NotifyObservers(victim);
        }

        /*
        "Decays" (decreases) the _score by 1 percent IF the score is greater than 400
        */

        public void DecayScore()
        {
            if (_score > _allowedScoreBeforeDecay)
            {
                _score -= (_score * _scoreDecayFactor);
            }
        }
    }
}