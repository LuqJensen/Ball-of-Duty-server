using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Bullet : GameObject
    {
        public int Damage { get; set; }
        public int OwnerId { get; set; }

        public Bullet(Point position, Vector velocity, double radius, int damage, int ownerId)
        {
            OwnerId = ownerId;
            Damage = damage;
            Body = new Body(this, position, radius, radius) { Type = Body.Geometry.RECTANGLE };
            Physics = new Physics(this, 200, velocity);
        }

        public override string ToString()
        {
            return $"[[Damage: {Damage}] [Owner: {OwnerId}] [Body: {Body}][Id: {Id}]";
        }
    }
}