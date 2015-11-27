using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations
{
    public class Blaster : Character
    {
        private const int HP = 100;
        private const double SIZE = 50;

        public Blaster() : base(SIZE, HP, Specializations.BLASTER)
        {
        }
    }
}