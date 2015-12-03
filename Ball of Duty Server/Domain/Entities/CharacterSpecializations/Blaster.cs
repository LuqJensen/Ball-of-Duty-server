using System;
using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations
{
    public class Blaster : Character
    {
        private const int HP = 100;
        private const double BASESIZE = 50;
        private const double HP_INCREASE_FACTOR = 0.1;
        private const int HP_REGEN = 5;

        public override int BaseHealth
        {
            get { return HP; }
        }

        public override double HealthIncreaseFactor
        {
            get { return HP_INCREASE_FACTOR; }
        }

        public Blaster() : base(BASESIZE, HP, Specializations.BLASTER, HP_REGEN)
        {
        }
    }
}