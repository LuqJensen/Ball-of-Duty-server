using System;
using System.Collections.Generic;
using System.Windows;
using Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations;
using Ball_of_Duty_Server.Domain.GameObjects;
using Ball_of_Duty_Server.Domain.GameObjects.Components;
using Ball_of_Duty_Server.Domain.GameObjects.Components.Physics.Collision;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public abstract class Character : GameObject, ICollidable
    {
        public double Score { get; set; } = 0;
        public Specializations Specialization { get; private set; }
        public double HighScore { get; set; } = 0;
        public abstract int BaseHealth { get; }
        public abstract double HealthIncreaseFactor { get; }

        private const double SCORE_UP = 60;
        private const double SCORE_UP_FACTOR = 0.5;
        private const double SCORE_DECAY_FACTOR = 0.01;
        private const double ALLOWED_SCORE_BEFORE_DECAY = 400;
        private int _killCount = 0;
        private const long DECAY_SCORE_INTERVAL = 5000;
        private const long REGEN_INTERVAL = 5000;
        private readonly LightEvent _decayScoreEvent;
        private readonly LightEvent _regenEvent;

        protected Character(double baseSize, int health, Specializations specialization, int baseHealthRegen)
        {
            Body = new Body(this, new Point(-500, -500), baseSize, baseSize)
            {
                Type = Body.Geometry.CIRCLE
            };
            Health = new Health(this, health, baseHealthRegen);
            Specialization = specialization;
            _decayScoreEvent = new LightEvent(DECAY_SCORE_INTERVAL, DecayScore);
            _regenEvent = new LightEvent(REGEN_INTERVAL, Health.RegenHealth);
            Type = EntityType.CHARACTER;
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
            UpdateStats();
            NotifyObservers(Observation.ACQUISITION_OF_GOLD, victim);
        }


        /// <summary>
        /// "Decays" (decreases) the Score by 1 percent IF the score is greater than 400
        /// </summary>
        private void DecayScore()
        {
            if (Score > ALLOWED_SCORE_BEFORE_DECAY)
            {
                Score -= (Score * SCORE_DECAY_FACTOR);
            }
            UpdateStats();
        }


        public void CollideWith(ICollidable other)
        {
        }

        public bool CollisionCriteria(ICollidable other)
        {
            return false;
        }

        public bool IsCollidingSpecial(ICollidable other)
        {
            return false;
        }

        public override void Update(long deltaTime, ICollection<GameObject> values)
        {
            _decayScoreEvent.Update(deltaTime);
            _regenEvent.Update(deltaTime);
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

        public void UpdateStats()
        {
            Health.Max = BaseHealth + (int)(Score * HealthIncreaseFactor);
        }
    }
}