using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ball_of_Duty_Server
{
    public class View
    {
        public double Scale
        {
            get;

            set;
        }

        public System.Drawing.Color Color
        {
            get;

            set;
        }

        public Domain.GameObject GameObject
        {
            get;

            set;
        }
    }
}