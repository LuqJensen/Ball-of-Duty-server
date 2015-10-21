using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    public class Character : ServerGameObject
    {
        public Character(int id) : base (id)
        {
            setBody(new ServerBody(this,new Point(0, 0)));
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