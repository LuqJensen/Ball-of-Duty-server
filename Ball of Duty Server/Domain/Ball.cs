using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    public class Ball : GameObject
    {
        protected Ball()
        {
            throw new System.NotImplementedException();
        }

        public int Radius
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
            }
        }

        public Vector Velocity
        {
            get { throw new NotImplementedException();}
            set { }
        }
    }
}