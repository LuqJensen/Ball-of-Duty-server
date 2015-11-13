using Ball_of_Duty_Server.Domain.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Utility
{
    public class Observable
    {
        private ConcurrentDictionary<IObserver, bool> _observers;

        public Observable()
        {
            _observers = new ConcurrentDictionary<IObserver, bool>();
        }

        public void Register(IObserver observer)
        {
            _observers.TryAdd(observer, true);
        }

        public void UnRegister(IObserver observer)
        {
            bool b;
            _observers.TryRemove(observer, out b);
        }

        protected void NotifyObservers()
        {
            foreach (IObserver i in _observers.Keys)
            {
                i.Update(this);
            }
        }

        protected void NotifyObservers(object data)
        {
            foreach (IObserver i in _observers.Keys)
            {
                i.Update(this, data);
            }
        }
    }
}