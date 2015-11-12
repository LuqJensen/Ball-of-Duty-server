using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ball_of_Duty_Server.Domain.Physics.Collision;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Bullet : GameObject, ICollidable
    {
        public int Damage { get; set; }
        public int OwnerId { get; set; }

        public Bullet(Point position, Vector velocity, double radius, int damage, int ownerId)
        {
            OwnerId = ownerId;
            Damage = damage;
            Body = new Body(this, position, radius, radius) { Type = Body.Geometry.RECTANGLE };
            Physics = new Physics.Physics(this, 200, velocity);
        }

        public override void Update(ICollection<GameObject> values)
        {
            if (Destroyed)
                return;

            Physics.Update();
            Physics.UpdateWithCollision(values);
        }

        public void CollideWith(ICollidable other)
        {
            if (other is Character)
            {
                Character victim = (Character)other;
                if (victim.Id == OwnerId)
                    return;

                victim.Health.TakeDamage(Damage, OwnerId);
                Destroy();
            }
            else if (other is Wall)
            {
                Destroy();
            }
        }

        public override string ToString()
        {
            return $"[[Damage: {Damage}] [Owner: {OwnerId}] [Body: {Body}][Id: {Id}]";
        }
    }
}