using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.Entities;

namespace Entity.Factories
{
    public interface ICharacterFactory
    {
        ICharacter CreateCharacter(int specialization);
    }
}