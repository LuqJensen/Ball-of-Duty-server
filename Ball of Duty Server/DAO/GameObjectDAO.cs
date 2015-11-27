using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.DAO
{
    public class GameObjectDAO
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double Score { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int Id { get; set; }

        public int Specialization { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
    }
}