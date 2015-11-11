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

        public virtual void Update(ICollection<GameObject> values)
        {
        }

        public void Destroy()
        {
            if (Destroyed)
                return;

            Destroyed = true;
            NotifyObservers();
        }

        public void Destroy(int killerId)
        {
            if (Destroyed)
                return;

            Destroyed = true;
            NotifyObservers(killerId);
        }
    }
}