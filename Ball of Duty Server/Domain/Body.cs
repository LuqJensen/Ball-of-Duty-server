using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    [Serializable]
    public class ServerBody
    {
        public ServerGameObject gameObject;
        private Point position;
        public ServerBody(ServerGameObject sgo, Point position)
        {
            gameObject = sgo;
        }

        public void setPosition(Point position)
        {
            this.position = position;
        }
        public Point getPosition()
        {
            return position;
        }


    }
}