using Entity.Components.Physics.Collision;

namespace Entity.Entities
{
    public interface IBullet : IGameObject, ICollidable
    {
        int Damage { get; set; }
        IGameObject Owner { get; set; }
        int BulletType { get; set; }
        int WallId { get; set; }
    }
}