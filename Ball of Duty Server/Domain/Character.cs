using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    public class Character : GameObject
    {
        public Character(int id) : base (id)
        {
            ObjectBody = new Body(this, new Point(0,0), 20, 20); // TODO should be dynamic
            ObjectBody.Type = Body.Geometry.CIRCLE;
        }
    }
}