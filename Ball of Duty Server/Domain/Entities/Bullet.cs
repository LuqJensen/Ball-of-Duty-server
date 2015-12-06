using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ball_of_Duty_Server.Domain.Physics.Collision;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Bullet : GameObject, ICollidable
    {
        private LightEvent _lifeTimeEvent;
        public int Damage { get; set; }
        public GameObject Owner { get; set; }

        public int BulletType { get; set; }

        public Bullet(Point position, Vector velocity, double radius, int damage, int bulletType, GameObject owner)
        {
            BulletType = bulletType;
            Owner = owner;
            Damage = damage;
            Body = new Body(this, position, radius, radius)
            {
                Type = Body.Geometry.CIRCLE
            };
            Physics = new Physics.Physics(this, 200, velocity);
            _lifeTimeEvent = new LightEvent(5000, DestroyEvent);
            Type = EntityType.BULLET;
        }

        private void DestroyEvent()
        {
            Destroy();
        }

        public override void Update(long deltaTime, ICollection<GameObject> values)
        {
            _lifeTimeEvent.Update(deltaTime); // this may destroy the bullet.

            if (Destroyed)
                return;

            Physics.Update(deltaTime);
            Physics.UpdateWithCollision(values);
        }

        public void CollideWith(ICollidable other)
        {
            if (other is Character)
            {
                Character victim = (Character)other;
                if (victim.Id == Owner.Id)
                    return;

                victim.Health.TakeDamage(Damage, Owner);
                Destroy();
            }
            else if (other is Wall)
            {
                Destroy();
            }
        }

        public override string ToString()
        {
            return $"[[Damage: {Damage}] [Owner: {Owner}] [Body: {Body}][Id: {Id}]";
        }
    }
}