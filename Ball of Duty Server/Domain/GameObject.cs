using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Ball_of_Duty_Server.Domain
{
    public abstract class GameObject
    {
        public GameObject()
        {
            throw new System.NotImplementedException();
        }

        public Point Position
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
            }
        }

        public Color Color
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
            }
        }

        public int Id
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
            }
        }
    }
}