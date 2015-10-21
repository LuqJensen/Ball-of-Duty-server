using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    [Serializable]
    public class Physics
    {
        public Physics()
        {
            throw new System.NotImplementedException();
        }

        public Vector Velocity { get; set; }

        public ServerGameObject GameObject { get; set; }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void ChangeVelocity()
        {
            throw new System.NotImplementedException();
        }
    }
}