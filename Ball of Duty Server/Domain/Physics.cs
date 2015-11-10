using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Ball_of_Duty_Server.Domain.Entities;

namespace Ball_of_Duty_Server.Domain
{
    public class Physics
    {
        public Vector Velocity { get; set; }

        public GameObject GameObject { get; set; }
        private double _topSpeed;

        private long _lastUpdate;

        private const int NANOSECOND_TO_SECOND = 10000000;

        public Physics(GameObject gameObject, double topSpeed, Vector velocity)
        {
            _lastUpdate = DateTime.Now.Ticks;
            GameObject = gameObject;
            _topSpeed = topSpeed;
            Velocity = velocity;
        }


        public void UpdateWithCollision(ICollection<GameObject> gameObjects)
        {
            double secondsSinceLast = ((double)DateTime.Now.Ticks - _lastUpdate) / NANOSECOND_TO_SECOND;
            _lastUpdate = DateTime.Now.Ticks;

            Point p = GameObject.Body.Position;
            p.Offset(Velocity.X * secondsSinceLast, Velocity.Y * secondsSinceLast);
            //because Point is a value type (struct).as if it is immutable

            GameObject.Body.Position = p;
            foreach (var other in gameObjects)
            {
                if (other is Bullet || GameObject.Destroyed)
                {
                    continue;
                }
                Bullet bullet = GameObject as Bullet;
                if (bullet != null)
                {
                    if (bullet.OwnerId != other.Id && CollisionHandler.IsColliding(bullet, other))
                    {
                        other.Health.TakeDamage(bullet.Damage);
                        bullet.Destroy();
                    }
                }
            }
        }
    }
}