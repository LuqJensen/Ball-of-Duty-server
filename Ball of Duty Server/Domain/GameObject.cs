using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;

namespace Ball_of_Duty_Server.Domain
{
    public  class GameObject
    {
        public Body Body { get; set; }
        public int Id { get; set; }

        public GameObject(int id)
        {
            Id = id;
            Body = new Body(new Point(0,0));
        }

        public void Destroy()
        {

        }
    }
}