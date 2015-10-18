using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.DTO
{
    public struct ObjectPosition
    {
        public int Id { get; private set; }
        public Point Position { get; private set; }

        public ObjectPosition(int id, Point position)
        {
            Id = id;
            Position = position;
        }
    }
}
