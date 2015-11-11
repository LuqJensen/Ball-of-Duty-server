using System;
using System.Windows;
using Ball_of_Duty_Server.Domain.Physics.Collision;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Character : GameObject, ICollidable
    {
        public double Score { get; set; } = 0;
        public const int MAX_HP = 107;

        private double _scoreUP = 100;
        private double _scoreUPFactor = 0.1;
        private double _scoreDecayFactor = 0.01;
        private double _allowedScoreBeforeDecay = 400;
        private int _killCount = 0;


        public Character()
        {
            Body = new Body(this, new Point(0, 0), 50, 50) { Type = Body.Geometry.CIRCLE }; // TODO should be dynamic
            Health = new Health(this, MAX_HP);
        }

        /// <summary>
        /// Called when character gets a kill 
        /// Increment killCount 
        /// Increment score with _scoreUP and _scoreFactor percent of the victims score 
        /// Notifies its observers (only player should react)  
        /// </summary>
        /// <param name="victim"></param>
        public void AddKill(Character victim)
        {
            ++_killCount;
            Score += _scoreUP + (victim.Score * _scoreUPFactor);
            NotifyObservers(victim);
        }

        /// <summary>
        /// "Decays" (decreases) the Score by 1 percent IF the score is greater than 400
        /// </summary>
        public void DecayScore()
        {
            if (Score > _allowedScoreBeforeDecay)
            {
                Score -= (Score * _scoreDecayFactor);
            }
        }

        public void CollideWith(ICollidable other)
        {
        }
    }
}