using System.Windows;
using Ball_of_Duty_Server.Domain.Physics.Collision;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Wall : GameObject, ICollidable
    {
        public Wall(Point position, int size)
        {
            Body = new Body(this, position, size, size) { Type = Body.Geometry.RECTANGLE };
        }

        public void CollideWith(ICollidable other)
        {
            
        }
    }
}