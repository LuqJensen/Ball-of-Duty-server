using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations
{
    public class Heavy : Character
    {
        private const int HP = 150;
        private const double SIZE = 50;

        public Heavy() : base(SIZE, HP)
        {
        }
    }
}