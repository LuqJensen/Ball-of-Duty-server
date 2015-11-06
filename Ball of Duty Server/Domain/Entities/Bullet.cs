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
        public double Damage { get; set; }
        public double OwnerId { get; set; }

        public Bullet(Point position, double radius, double damage, int ownerId)
        {
            OwnerId = ownerId;
            Damage = damage;
            Console.WriteLine($"Bullet created: {this}");
            Body = new Body(this, position, radius, radius) { Type = Body.Geometry.RECTANGLE };
        }

        public override string ToString()
        {
            return $"[[Damage: {Damage}] [Owner: {OwnerId}] [Body: {Body}][Id: {Id}]";
        }
    }
}