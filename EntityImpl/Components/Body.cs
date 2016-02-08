using System.Windows;
using Entity;
using Entity.Components;
using Entity.DTO;

namespace EntityImpl.Components
{
    public class Body : IBody
    {
        public IGameObject GameObject { get; set; }
        public Point Position { get; set; }
        public double Width { get; set; }
        public Point Center => new Point(Position.X + Width / 2, Position.Y + Height / 2);
        public double Height { get; set; }
        public Geometry Type { get; set; }

        public Body(IGameObject go, Point position, double width, double height)
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