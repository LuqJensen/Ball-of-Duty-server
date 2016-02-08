using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.Entities;
using Entity.Entities.CharacterSpecializations;
using Entity.Factories;
using EntityImpl.Entities.CharacterSpecializations;

namespace EntityImpl.Factories
{
    public class CharacterFactory : ICharacterFactory
    {
        private Dictionary<Specializations, Func<ICharacter>> _specializations =
            new Dictionary<Specializations, Func<ICharacter>>();


        public CharacterFactory()
        {
            _specializations.Add(Specializations.ROLLER, CreateRoller);
            _specializations.Add(Specializations.BLASTER, CreateBlaster);
            _specializations.Add(Specializations.HEAVY, CreateHeavy);
        }

        public ICharacter CreateCharacter(int specialization)
        {
            if (!Enum.IsDefined(typeof (Specializations), specialization))
            {
                throw new ArgumentException($"Could not resolve {specialization} to a valid {typeof (Specializations)}");
            }
            // C# int to enum casts never throw an exception, this is due to bitfields.
            // http://stackoverflow.com/questions/1758321/casting-ints-to-enums-in-c-sharp
            return _specializations[(Specializations)specialization]();
        }

        private ICharacter CreateRoller()
        {
            Roller roller = new Roller();
            return roller;
        }

        private ICharacter CreateBlaster()
        {
            Blaster blaster = new Blaster();
            return blaster;
        }

        private ICharacter CreateHeavy()
        {
            Heavy heavy = new Heavy();
            return heavy;
        }
    }
}