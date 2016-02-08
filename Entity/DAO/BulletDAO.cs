using System.Windows;

namespace Entity.DAO
{
    public class BulletDAO
    {
        public Point Position { get; set; }
        public Vector Vector { get; set; }
        public int Damage { get; set; }
        public int BulletType { get; set; }
        public IGameObject Owner { get; set; }
        public double Width { get; set; }
        public int Id { get; set; }
        public int Type { get; set; }
    }
}