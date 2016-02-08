using System.Collections.Generic;
using System.Windows;
using GameObject.DTO;

namespace GameObject.GameObjects.Components.Physics
{
    public interface IPhysics
    {
        Vector Velocity { get; set; }
        IGameObject GameObject { get; set; }

        void Update(long deltaTime);

        void UpdateWithCollision(ICollection<IGameObject> gameObjects);

        PhysicsDTO Export();
    }
}