using Entity.Entities.CharacterSpecializations;

namespace EntityImpl.Entities.CharacterSpecializations
{
    public class Blaster : Character
    {
        private const int HP = 100;
        private const double BASESIZE = 50;
        private const double HP_INCREASE_FACTOR = 0.1;
        private const int HP_REGEN = 5;

        protected override sealed int BaseHealth => HP;

        protected override sealed double HealthIncreaseFactor => HP_INCREASE_FACTOR;

        public Blaster() : base(BASESIZE, HP, Specializations.BLASTER, HP_REGEN)
        {
        }
    }
}