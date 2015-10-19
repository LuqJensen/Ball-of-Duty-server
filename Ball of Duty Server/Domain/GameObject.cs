using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;

namespace Ball_of_Duty_Server.Domain
{
    public abstract class GameObject
    {

        public int Id { get; set; }

        public View View { get; set; }

        public Physics Physics { get; set; }

        public Body Body { get; set; }
    }
}