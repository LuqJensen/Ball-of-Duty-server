using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    public class Character : GameObject
    {
        public Character(int id)
        {
            Id = id;
            Body = new Body(new Point(0, 0));
        }

        public int MovementSpeed
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