using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public static class CharacterFactory
    {
        private static Dictionary<Specializations, Func<Character>> _specializations =
            new Dictionary<Specializations, Func<Character>>();


        static CharacterFactory()
        {
            _specializations.Add(Specializations.ROLLER, CharacterFactory.CreateRoller);
            _specializations.Add(Specializations.BLASTER, CharacterFactory.CreateBlaster);
            _specializations.Add(Specializations.HEAVY, CharacterFactory.CreateHeavy);
        }

        public static Character CreateCharacter(Specializations specialization)
        {
            Func<Character> f;

            if (!_specializations.TryGetValue(specialization, out f))
            {
                return null;
            }

            Character c = f();
            return c;
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