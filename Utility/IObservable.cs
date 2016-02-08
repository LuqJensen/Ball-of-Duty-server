using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public interface IObservable
    {
        void Register(Observation observation, object observer, Action<IObservable, object> action);

        void Unregister(Observation observation, object observer);

        void UnregisterAll(object observer);
    }
}