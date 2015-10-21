using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server
{
    [ServiceContract]
    interface ISimple
    {
        [OperationContract]
        int[] getArray();
    }
}
