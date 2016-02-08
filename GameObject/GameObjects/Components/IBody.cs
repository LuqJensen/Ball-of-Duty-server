using System.Windows;
using GameObject.DTO;

namespace GameObject.GameObjects.Components
{
    public interface IBody
    {
        IGameObject GameObject { get; set; }
        Point Position { get; set; }
        double Width { get; set; }
        Point Center { get; }
        double Height { get; set; }
        Geometry Type { get; set; }

        BodyDTO Export();
    }
}