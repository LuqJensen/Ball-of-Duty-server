using System;
using System.Collections.Generic;
using System.Windows;
using Entity;
using Entity.Components;
using Entity.Components.Physics.Collision;
using Entity.DTO;
using Entity.Entities;
using EntityImpl.Components;
using EntityImpl.Components.Physics;
using EntityImpl.Components.Physics.Collision;
using Utility;

namespace EntityImpl.Entities
{
    public class Bullet : GameObject, IBullet
    {
        private LightEvent _lifeTimeEvent;
        public int Damage { get; set; }
        public IGameObject Owner { get; set; }

        public int BulletType { get; set; }

        public int WallId { get; set; }

        public Bullet(Point position, Vector velocity, double diameter, int damage, int bulletType, IGameObject owner)
        {
            BulletType = bulletType;
            Owner = owner;
            Damage = damage;
            Body = new Body(this, position, diameter, diameter)
            {
                Type = Geometry.CIRCLE
            };
            Physics = new Physics(this, 200, velocity);
            _lifeTimeEvent = new LightEvent(5000, () => { Destroy(); });
            Type = EntityType.BULLET;
        }

        public override void Update(long deltaTime, ICollection<IGameObject> values)
        {
            _lifeTimeEvent.Update(deltaTime); // this may destroy the bullet.

            if (Destroyed)
                return;

            Physics.Update(deltaTime);
            Physics.UpdateWithCollision(values, new CollisionHandler()); // TODO fix
        }

        public override GameObjectDTO Export()
        {
            return new GameObjectDTO
            {
                Body = Body.Export(),
                BulletType = BulletType,
                Id = Id,
                Type = (int)Type,
                Physics = Physics.Export()
            };
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

        public bool CollisionCriteria(ICollidable other)
        {
            if (other is Wall)
            {
                return ((Wall)other).Id == WallId;
            }
            if (other is Character)
            {
                return true;
            }
            return false;
        }

        public bool IsCollidingSpecial(ICollidable other)
        {
            if (!(other is Wall))
            {
                return false;
            }

            // TODO move below logic somewhere else...
            Wall wall = (Wall)other;

            Point center1 = Body.Center;
            Vector velocity = Physics.Velocity;
            double xPlus = velocity.X < 0 ? -1 : 1;
            double yPlus = velocity.Y < 0 ? -1 : 1;

            Point center2 = wall.Body.Center;
            double dx = Math.Abs(center1.X - center2.X);
            double dy = Math.Abs(center1.Y - center2.Y);

            double distanceBefore = Math.Sqrt((dx * dx) + (dy * dy));

            dx = Math.Abs((center1.X + xPlus) - (center2.X));
            dy = Math.Abs((center1.Y + yPlus) - (center2.Y));

            double distanceAfter = Math.Sqrt((dx * dx) + (dy * dy));

            return distanceAfter > distanceBefore;
        }

        public override string ToString()
        {
            return $"[[Damage: {Damage}] [Owner: {Owner}] [Body: {Body}][Id: {Id}]";
        }
    }
}