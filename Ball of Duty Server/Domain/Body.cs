using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    public class Body
    {
        public Point Position { get; set; }

        public Body(Point position)
        {
            Position = position;
        }
    }
}