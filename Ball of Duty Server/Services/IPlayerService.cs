using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Ball_of_Duty_Server.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPlayerService" in both code and config file together.
    [ServiceContract]
    public interface IPlayerService
    {
        [OperationContract]
        void DoWork();
    }
}
