using Ball_of_Duty_Server.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Domain
{
    public interface IObserver
    {
        void Update(Observable observable);

        void Update(Observable observable, object data);
    }
}
