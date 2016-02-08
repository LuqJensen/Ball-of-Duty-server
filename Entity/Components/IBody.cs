using System.Windows;
using Entity.DTO;

namespace Entity.Components
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