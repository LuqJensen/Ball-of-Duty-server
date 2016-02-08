using System;
using System.Collections.Generic;
using System.Windows;
using Entity;
using Entity.DAO;
using Entity.DTO;
using Entity.Entities;
using Entity.Entities.CharacterSpecializations;
using Entity.Factories;
using EntityImpl.Entities;
using EntityImpl.Entities.CharacterSpecializations;

namespace EntityImpl.Factories
{
    [Serializable]
    public class EntityFactory : IEntityFactory
    {
        private CharacterFactory _characterFactory = new CharacterFactory();

        public IGameObject CreateGameObject(GameObjectDTO data)
        {
            if (data.Specialization > 0)
            {
                return _characterFactory.CreateCharacter(data.Specialization);
            }
            return null;
        }

        public ICharacter CreateCharacter(int specialization)
        {
            return _characterFactory.CreateCharacter(specialization);
        }

        public IWall CreateWall(Point position, int size)
        {
            return new Wall(position, size);
        }

        public IBullet CreateBullet(BulletDAO data)
        {
            return new Bullet(data.Position, data.Vector, data.Width, data.Damage, data.BulletType, data.Owner);
        }
    }
}