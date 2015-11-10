using System;
using System.Collections.Generic;

namespace Ball_of_Duty_Server.Domain.Entities
{
    public class GameObject : Observable
    {
        private static int _gameObjectsCreated;
        public Body Body { get; set; }
        public Health Health { get; set; }
        public Physics Physics { get; set; }
        public int Id { get; private set; }
        public bool Destroyed { get; private set; } = false;

        public GameObject()
        {
            Id = ++_gameObjectsCreated; // Important to start at 1. 0 will be used as default value.
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void UpdateWithCollision(ICollection<GameObject> values)
        {
            Physics.UpdateWithCollision(values);
        }

        public void Destroy()
        {
            Destroyed = true;
            NotifyObservers();
        }

        public void Destroy(int killerId)
        {
            Destroyed = true;
            NotifyObservers(killerId);
        }
    }
}