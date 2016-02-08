using System.Collections.Generic;
using System.Windows;
using Entity.Components.Physics.Collision;
using Entity.DTO;

namespace Entity.Components.Physics
{
    public interface IPhysics
    {
        Vector Velocity { get; set; }
        IGameObject GameObject { get; set; }
        void Update(long deltaTime);
        void UpdateWithCollision(ICollection<IGameObject> gameObjects, ICollisionHandler collisionHandler);
        PhysicsDTO Export();
    }
}