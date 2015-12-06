using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain.Entities;

namespace Ball_of_Duty_Server.DTO
{
    [Serializable]
    public struct GameObjectDTO
    {
        public int Id;
        public BodyDTO Body;
        public PhysicsDTO Physics;
        public int Type;
        public int BulletType;
        public int Specialization;
    }
}