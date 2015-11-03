using System.Windows;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class Character : GameObject
    {
        public Character()
        {
            Body = new Body(this, new Point(0, 0), 20, 20) { Type = Body.Geometry.CIRCLE }; // TODO should be dynamic
        }
    }
}