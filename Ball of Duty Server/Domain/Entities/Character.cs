﻿using System;
using System.Collections.Generic;
using System.Windows;
using Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations;
using Ball_of_Duty_Server.Domain.Physics.Collision;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public abstract class Character : GameObject, ICollidable
    {
        public double Score { get; set; } = 0;
        public Specializations Specialization { get; private set; }
        public double HighScore { get; set; } = 0;

        private const double SCORE_UP = 100;
        private const double SCORE_UP_FACTOR = 0.1;
        private const double SCORE_DECAY_FACTOR = 0.01;
        private const double ALLOWED_SCORE_BEFORE_DECAY = 400;
        private int _killCount = 0;
        private const long Decay_INTERVAL = 5000;
        private const long Regen_INTERVAL = 5000;
        private readonly LightEvent _decayEvent;
        private readonly LightEvent _regenEvent;

        public Character(double size, int health, Specializations specialization, int HealthRegen,
            double healthIncreaseFactor)
        {
            Body = new Body(this, new Point(150, 150), size, size) { Type = Body.Geometry.CIRCLE };
            // TODO should be dynamic
            Health = new Health(this, health, HealthRegen, healthIncreaseFactor);
            Specialization = specialization;
            _decayEvent = new LightEvent(Decay_INTERVAL, DecayScore());
            _regenEvent = new LightEvent(Regen_INTERVAL, RegenHealth());
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

        public void RegenHealth()
        {
            if (Health.Value < Health.Max)
            {
                Health.Value += Health.HealthRegen;

                if (Health.Value > Health.Max)
                {
                    Health.Value = Health.Max;
                }
            }
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

        public override void Update(long deltaTime, ICollection<GameObject> values)
        {
            _decayEvent.Update(deltaTime);
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
            Health.Max += (int)(Score * Health.HealthIncreaseFactor);
        }
    }
}