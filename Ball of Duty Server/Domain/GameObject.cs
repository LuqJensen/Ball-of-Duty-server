using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;

namespace Ball_of_Duty_Server.Domain
{
    public abstract class GameObject
    {

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

        public View View
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
            }
        }

        public Physics Physics
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
            }
        }

        public Body Body
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