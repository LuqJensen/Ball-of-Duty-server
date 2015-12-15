using System.Windows;
using Ball_of_Duty_Server.Domain.GameObjects;
using Ball_of_Duty_Server.Domain.GameObjects.Components;
using Ball_of_Duty_Server.Domain.GameObjects.Components.Physics.Collision;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Wall : GameObject, ICollidable
    {
        public Wall(Point position, int size)
        {
            Body = new Body(this, position, size, size)
            {
                Type = Body.Geometry.RECTANGLE
            };
            Type = EntityType.WALL;
        }

        public void CollideWith(ICollidable other)
        {
        }

        public bool CollisionCriteria(ICollidable other)
        {
            return false;
        }

        public bool IsCollidingSpecial(ICollidable other)
        {
            return false;
        }
    }
}