using System.Collections.Generic;
using Entity.Components;
using Entity.Components.Physics;
using Entity.DTO;
using Utility;

namespace Entity
{
    public interface IGameObject : IObservable
    {
        IBody Body { get; set; }
        IHealth Health { get; set; }
        IPhysics Physics { get; set; }
        int Id { get; }
        EntityType Type { get; }
        bool Destroyed { get; }

        void Update(long deltaTime, ICollection<IGameObject> values);

        bool Destroy();

        bool Destroy(IGameObject exterminator);

        GameObjectDTO Export();
    }
}