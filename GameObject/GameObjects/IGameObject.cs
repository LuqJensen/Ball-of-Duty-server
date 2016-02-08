using System.Collections.Generic;
using System.Threading;
using GameObject.DTO;
using GameObject.GameObjects.Components;
using GameObject.GameObjects.Components.Physics;

namespace GameObject.GameObjects
{
    public interface IGameObject
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