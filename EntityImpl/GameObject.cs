using System.Collections.Generic;
using System.Threading;
using Entity;
using Entity.Components;
using Entity.Components.Physics;
using Entity.DTO;
using Utility;
using UtilityImpl;

namespace EntityImpl
{
    public abstract class GameObject : Observable, IGameObject
    {
        private static int _gameObjectsCreated;
        public IBody Body { get; set; }
        public IHealth Health { get; set; }
        public IPhysics Physics { get; set; }
        public int Id { get; } = Interlocked.Increment(ref _gameObjectsCreated);
        public EntityType Type { get; protected set; }
        public bool Destroyed { get; private set; } = false;

        public virtual void Update(long deltaTime, ICollection<IGameObject> values)
        {
        }

        public bool Destroy()
        {
            if (Destroyed)
                return false;

            NotifyObservers(Observation.EXTERMINATION);
            Destroyed = true;
            return Destroyed;
        }

        public virtual bool Destroy(IGameObject exterminator)
        {
            if (Destroyed)
                return false;

            NotifyObservers(Observation.KILLING, exterminator);
            Destroyed = true;
            return Destroyed;
        }

        public abstract GameObjectDTO Export();
    }
}