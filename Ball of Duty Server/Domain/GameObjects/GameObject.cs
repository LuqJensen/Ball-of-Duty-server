using System.Collections.Generic;
using System.Threading;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Domain.GameObjects.Components;
using Ball_of_Duty_Server.Domain.GameObjects.Components.Physics;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Domain.GameObjects
{
    public class GameObject : Observable
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
    }
}