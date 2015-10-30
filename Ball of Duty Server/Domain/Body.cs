using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    public class Body
    {
        public GameObject gameObject;
        public Point Position { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Geometry Type { get; set; }

        public enum Geometry
        {
            CIRCLE = 0,
            RECTANGLE = 1
        }
        

        public Body(GameObject sgo, Point position, int width, int height)
        {
            this.Position = position;
            gameObject = sgo;
            this.Width = width;
            this.Height = height;
        }
    }
}