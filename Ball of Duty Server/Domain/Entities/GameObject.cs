using System;
using System.Collections.Generic;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class GameObject : Observable
    {
        private static int _gameObjectsCreated;
        public Body Body { get; set; }
        public Health Health { get; set; }
        public Physics.Physics Physics { get; set; }
        public int Id { get; private set; }
        public bool Destroyed { get; private set; } = false;

        public GameObject()
        {
            Id = ++_gameObjectsCreated; // Important to start at 1. 0 will be used as default value.
        }

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