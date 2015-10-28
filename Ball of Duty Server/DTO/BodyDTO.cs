using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ball_of_Duty_Server.DTO
{
    [Serializable]


    public struct BodyDTO
    {
        public int _width;
        public int _height;
        public int _type;
        public int CIRCLE;
        public int RECTANGLE;
        public PointDTO _point;
    }
}
