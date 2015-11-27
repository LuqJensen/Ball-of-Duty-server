using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.DTO
{
    [Serializable]
    public struct GameObjectDTO
    {
        public int Id;
        public BodyDTO Body;
        public int Specialization;
    }
}