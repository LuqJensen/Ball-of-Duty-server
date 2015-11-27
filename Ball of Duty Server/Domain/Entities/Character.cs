using System;
using System.Collections.Generic;
using System.Windows;
using Ball_of_Duty_Server.Domain.Physics.Collision;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Character : GameObject, ICollidable
    {
        public double Score { get; set; } = 0;
        public double HighScore { get; set; } = 0;

        private const double SCORE_UP = 100;
        private const double SCORE_UP_FACTOR = 0.1;
        private const double SCORE_DECAY_FACTOR = 0.01;
        private const double ALLOWED_SCORE_BEFORE_DECAY = 400;
        private int _killCount = 0;
        private const long SCORE_DECAY_INTERVAL = 5000;
        private readonly LightEvent _decayScoreEvent;

        public Character(double size, int health)
        {
            Body = new Body(this, new Point(0, 0), size, size) { Type = Body.Geometry.CIRCLE };
            // TODO should be dynamic
            Health = new Health(this, health);
            _decayScoreEvent = new LightEvent(SCORE_DECAY_INTERVAL, DecayScore);
        }

        /// <summary>
        /// Called when character gets a kill. 
        /// Increment killCount. 
        /// Increment score with _scoreUP and _scoreFactor percent of the victims score. 
        /// Notifies its observers (only player should react).  
        /// </summary>
        /// <param name="victim">The victim</param>
        public void AddKill(Character victim)
        {
            ++_killCount;
            Score += SCORE_UP + (victim.Score * SCORE_UP_FACTOR);
            if (Score > HighScore)
            {
                HighScore = Score;
            }
            NotifyObservers(Observation.ACQUISITION_OF_GOLD, victim);
        }

        /// <summary>
        /// "Decays" (decreases) the Score by 1 percent IF the score is greater than 400
        /// </summary>
        public void DecayScore()
        {
            if (Score > ALLOWED_SCORE_BEFORE_DECAY)
            {
                Score -= (Score * SCORE_DECAY_FACTOR);
            }
        }


        public void CollideWith(ICollidable other)
        {
        }

        public override void Update(long deltaTime, ICollection<GameObject> values)
        {
            _decayScoreEvent.Update(deltaTime);
        }

        public override bool Destroy(GameObject exterminator)
        {
            if (base.Destroy(exterminator))
            {
                Character killer = exterminator as Character;
                killer?.AddKill(this);
                return true;
            }
            return false;
        }
    }
}