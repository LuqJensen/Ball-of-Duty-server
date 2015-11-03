using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Wall : GameObject
    {
        public Wall(Point position, int size)
        {
            Body = new Body(this, position, size, size) { Type = Body.Geometry.RECTANGLE };
        }
    }
}