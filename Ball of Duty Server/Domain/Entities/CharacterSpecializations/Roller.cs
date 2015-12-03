using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations
{
    public class Roller : Character
    {
        private const int HP = 100;
        private const double SIZE = 50;
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

        public Roller() : base(SIZE, HP, Specializations.ROLLER, HP_REGEN)
        {
        }
    }
}