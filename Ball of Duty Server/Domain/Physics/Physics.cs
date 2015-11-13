using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Domain.Physics.Collision;

namespace Ball_of_Duty_Server.Domain.Physics
{
    public class Physics
    {
        private double _topSpeed;
        private long _lastUpdate;
        private const int MILLISECOND_TO_SECOND = 1000;

        public Vector Velocity { get; set; }

        public GameObject GameObject { get; set; }

        public Physics(GameObject gameObject, double topSpeed, Vector velocity)
        {
            _lastUpdate = DateTime.Now.Ticks;
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

                if (CollisionHandler.IsColliding(GameObject, other))
                {
                    ((ICollidable)GameObject).CollideWith((ICollidable)other);
                }
            }
        }
    }
}