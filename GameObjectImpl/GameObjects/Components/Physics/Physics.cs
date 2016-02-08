using System.Collections.Generic;
using System.Windows;
using GameObject.DTO;
using GameObject.GameObjects.Components.Physics.Collision;
using GameObjectImpl.GameObjects.Components.Physics.Collision;

namespace GameObjectImpl.GameObjects.Components.Physics
{
    public class Physics
    {
        private double _topSpeed;
        private const int MILLISECOND_TO_SECOND = 1000;

        public Vector Velocity { get; set; }

        public GameObject GameObject { get; set; }

        public Physics(GameObject gameObject, double topSpeed, Vector velocity)
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

        public void UpdateWithCollision(ICollection<GameObject> gameObjects)
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

                if (CollisionHandler.IsColliding(GameObject.Body, other.Body) || go1.IsCollidingSpecial(go2))
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