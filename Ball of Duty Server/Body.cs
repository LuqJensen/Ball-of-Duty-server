using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ball_of_Duty_Server
{
    public class Body
    {
        public Domain.GameObject GameObject
        {
            get;

            set;
        }

        public Point Position
        {
            get;

            set;
        }

        public Body(Point position)
        {
            Position = position;
        }

    }
}