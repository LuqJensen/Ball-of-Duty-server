using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ball_of_Duty_Server.DTO
{
    [Serializable]
    public class BodyDTO
    {
        public double Width;
        public double Height;
        public int Type;
        public PointDTO Position;
    }
}
