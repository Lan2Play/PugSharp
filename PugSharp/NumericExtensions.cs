namespace PugSharp
{
    internal static class NumericExtensions
    {
        private const double _HalfFactor = 0.5;
        public static int Half(this int value)
        {
            return (int)(value * _HalfFactor);
        }
    }
}
