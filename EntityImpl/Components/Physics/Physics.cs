using System.Collections.Generic;
using System.Windows;
using Entity;
using Entity.Components.Physics;
using Entity.Components.Physics.Collision;
using Entity.DTO;

namespace EntityImpl.Components.Physics
{
    public class Physics : IPhysics
    {
        private double _topSpeed;
        private const int MILLISECOND_TO_SECOND = 1000;

        public Vector Velocity { get; set; }

        public IGameObject GameObject { get; set; }

        public Physics(IGameObject gameObject, double topSpeed, Vector velocity)
        {
            GameObject = gameObject;
            _topSpeed = topSpeed;
            Velocity = velocity;
        }

        public void Update(long deltaTime)
        {
            double deltaSeconds = ((double)deltaTime) / MILLISECOND_TO_SECOND;

            Point p = GameObject.Body.Position;
            p.Offset(Velocity.X * deltaSeconds, Velocity.Y * deltaSeconds);
            //because Point is a value type (struct).as if it is immutable

            GameObject.Body.Position = p;
        }

        public void UpdateWithCollision(ICollection<IGameObject> gameObjects, ICollisionHandler collisionHandler)
        {
            if (!(GameObject is ICollidable))
                return;

            foreach (var other in gameObjects)
            {
                if (GameObject.Destroyed)
                {
                    return;
                }

                if (other.Destroyed || !(other is ICollidable) || other.Id == GameObject.Id)
                {
                    continue;
                }

                ICollidable go1 = (ICollidable)GameObject;
                ICollidable go2 = (ICollidable)other;

                if (!go1.CollisionCriteria(go2))
                {
                    continue;
                }

                if (collisionHandler.IsColliding(GameObject.Body, other.Body) || go1.IsCollidingSpecial(go2))
                {
                    (go1).CollideWith(go2);
                }
            }
        }

        public PhysicsDTO Export()
        {
            return new PhysicsDTO
            {
                VelocityX = Velocity.X,
                VelocityY = Velocity.Y
            };
        }
    }
}