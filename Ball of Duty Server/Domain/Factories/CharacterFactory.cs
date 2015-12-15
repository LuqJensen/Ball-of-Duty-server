using System;
using System.Collections.Generic;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations;

namespace Ball_of_Duty_Server.Domain.Factories
{
    public static class CharacterFactory
    {
        private static Dictionary<Specializations, Func<Character>> _specializations =
            new Dictionary<Specializations, Func<Character>>();


        static CharacterFactory()
        {
            _specializations.Add(Specializations.ROLLER, CreateRoller);
            _specializations.Add(Specializations.BLASTER, CreateBlaster);
            _specializations.Add(Specializations.HEAVY, CreateHeavy);
        }

        public static Character CreateCharacter(Specializations specialization)
        {
            return _specializations[specialization]();
        }

        private static Character CreateRoller()
        {
            Roller roller = new Roller();
            return roller;
        }

        private static Character CreateBlaster()
        {
            Blaster blaster = new Blaster();
            return blaster;
        }

        private static Character CreateHeavy()
        {
            Heavy heavy = new Heavy();
            return heavy;
        }
    }
}