using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Utility
{
    public static class ComparisonExtensions
    {
        public static bool IsInRange(this double value, double start, double end)
        {
            return (value >= start) && (value <= end);
        }
    }
}