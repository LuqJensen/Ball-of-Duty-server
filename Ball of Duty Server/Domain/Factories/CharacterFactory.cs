using System;
using System.Collections.Generic;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations;
using Ball_of_Duty_Server.DTO;

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

        public static Character CreateCharacter(int specialization)
        {
            if (!Enum.IsDefined(typeof (Specializations), specialization))
            {
                throw new ArgumentException($"Could not resolve {specialization} to a valid {typeof (Specializations)}");
            }
            // C# int to enum casts never throw an exception, this is due to bitfields.
            // http://stackoverflow.com/questions/1758321/casting-ints-to-enums-in-c-sharp
            return _specializations[(Specializations)specialization]();
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