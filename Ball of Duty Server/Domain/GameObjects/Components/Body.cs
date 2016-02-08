using System.Windows;
using Ball_of_Duty_Server.DTO;

namespace Ball_of_Duty_Server.Domain.GameObjects.Components
{
    public class Body
    {
        public GameObject GameObject { get; set; }
        public Point Position { get; set; }
        public double Width { get; set; }
        public Point Center => new Point(Position.X + Width / 2, Position.Y + Height / 2);
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

        public BodyDTO Export()
        {
            return new BodyDTO
            {
                Height = Height,
                Position = new PointDTO
                {
                    X = Position.X,
                    Y = Position.Y
                },
                Type = (int)Type,
                Width = Width
            };
        }
    }
}