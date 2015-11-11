using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Domain.Physics.Collision
{
    public interface ICollidable
    {
        void CollideWith(ICollidable other);
    }
}