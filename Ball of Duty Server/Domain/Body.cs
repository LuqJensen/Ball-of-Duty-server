using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Ball_of_Duty_Server.Domain.Entities;

namespace Ball_of_Duty_Server.Domain
{
    public class Body
    {
        public GameObject GameObject { get; set; }
        public Point Position { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Geometry Type { get; set; }

        public enum Geometry
        {
            CIRCLE = 0,
            RECTANGLE = 1
        }


        public Body(GameObject go, Point position, double width, double height)
        {
            this.Position = position;
            this.GameObject = go;
            this.Width = width;
            this.Height = height;
        }

        public override string ToString()
        {
            return $"[[Position: {Position.X},{Position.Y}] [Height: {Height}] [Width = {Width}]]";
        }
    }
}