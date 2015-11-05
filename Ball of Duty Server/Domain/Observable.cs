using Ball_of_Duty_Server.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Domain
{
    public class Observable
    {
        private List<IObserver> _observers;

        public void Register(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void UnRegister(IObserver observer)
        {
            _observers.Remove(observer);
        }

        protected void NotifyObservers()
        {
            foreach (IObserver i in _observers)
            {
                i.Update(this);
            }

        }

        protected void NotifyObservers(object data)
        {
            foreach (IObserver i in _observers)
            {
                i.Update(this, data);
            }
        }
    }
}
