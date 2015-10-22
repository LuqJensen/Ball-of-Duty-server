using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.DTO
{
    [Serializable]
    public struct CharacterDTO
    {
        public int Id;
        public BodyDTO Body;
    }
}
