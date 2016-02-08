namespace Utility
{
    public static class ComparisonExtensions
    {
        public static bool IsInRange(this double value, double start, double end)
        {
            return (value >= start) && (value <= end);
        }
    }
}