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
        public int Width;
        public int Height;
        public int Type;
        public PointDTO Position;
    }
}
