using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    public class Wall : GameObject
    {
        public static int wallsCreated = 0;
        public Wall(Point position, int size) : base(wallsCreated+1000)
        {
            wallsCreated++;

            ObjectBody = new Body(this, position, size, size);
            ObjectBody.Type = Body.Geometry.RECTANGLE;
        }
    }
}