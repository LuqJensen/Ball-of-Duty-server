using System.Windows;
using Entity.DAO;
using Entity.DTO;
using Entity.Entities;

namespace Entity.Factories
{
    public interface IEntityFactory
    {
        IGameObject CreateGameObject(GameObjectDTO data);
        IWall CreateWall(Point position, int size);
        IBullet CreateBullet(BulletDAO data);
        ICharacter CreateCharacter(int specialization);
    }
}