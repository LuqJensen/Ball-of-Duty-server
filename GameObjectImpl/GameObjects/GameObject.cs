using System.Collections.Generic;
using System.Threading;
using GameObject.DTO;
using GameObject.GameObjects;
using GameObjectImpl.GameObjects.Components;
using GameObjectImpl.GameObjects.Components.Physics;
using Utility;

namespace GameObjectImpl.GameObjects
{
    public abstract class GameObject : Observable
    {
        private static int _gameObjectsCreated;
        public Body Body { get; set; }
        public Health Health { get; set; }
        public Physics Physics { get; set; }
        public int Id { get; } = Interlocked.Increment(ref _gameObjectsCreated);
        public EntityType Type { get; protected set; }
        public bool Destroyed { get; private set; } = false;

        public virtual void Update(long deltaTime, ICollection<GameObject> values)
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

        public virtual bool Destroy(GameObject exterminator)
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