using System;
using System.Collections.Concurrent;

namespace Utility
{
    public class Observable
    {
        private static readonly Observation[] _observations = (Observation[])Enum.GetValues(typeof (Observation));

        private readonly ConcurrentDictionary<Observation, ConcurrentDictionary<object, Action<Observable, object>>>
            _observers;

        public Observable()
        {
            _observers =
                new ConcurrentDictionary<Observation, ConcurrentDictionary<object, Action<Observable, object>>>();
            foreach (Observation v in _observations)
            {
                _observers.TryAdd(v, new ConcurrentDictionary<object, Action<Observable, object>>());
            }
        }


        public void Register(Observation observation, object observer, Action<Observable, object> action)
        {
            _observers[observation].TryAdd(observer, action);
        }

        public void Unregister(Observation observation, object observer)
        {
            Action<Observable, object> action;
            _observers[observation].TryRemove(observer, out action);
        }

        public void UnregisterAll(object observer)
        {
            foreach (var v in _observers.Values)
            {
                Action<Observable, object> action;
                v.TryRemove(observer, out action);
            }
        }

        protected void NotifyObservers(Observation observation, object data = null)
        {
            foreach (var i in _observers[observation])
            {
                i.Value(this, data);
            }
        }
    }
}