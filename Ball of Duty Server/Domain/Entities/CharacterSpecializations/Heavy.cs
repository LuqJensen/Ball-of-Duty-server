﻿using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations
{
    public class Heavy : Character
    {
        private const int HP = 150;
        private const double SIZE = 50;
        private const double HP_INCREASE_FACTOR = 0.2;
        private const int HP_REGEN = 7;

        public Heavy() : base(SIZE, HP, Specializations.HEAVY, HP_REGEN, HP_INCREASE_FACTOR)
        {
        }
    }
}