using System.Windows;
using Entity;
using Entity.Components;
using Entity.Components.Physics.Collision;
using Entity.DTO;
using Entity.Entities;
using EntityImpl.Components;

namespace EntityImpl.Entities
{
    public class Wall : GameObject, IWall
    {
        public Wall(Point position, int size)
        {
            Body = new Body(this, position, size, size)
            {
                Type = Geometry.RECTANGLE
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

        public override GameObjectDTO Export()
        {
            return new GameObjectDTO
            {
                Body = Body.Export(),
                Id = Id,
                Type = (int)Type
            };
        }
    }
}