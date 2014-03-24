namespace Potassium.Utilities
{
    using System;

    /// <summary>
    /// Helper class for generating instances Hz 
    /// </summary>
    public static class Frequency
    {
        public static Hz Hz(TimeSpan ts)
        {
            return new Hz(ts);
        }

        public static Hz Hz(double hz)
        {
            return new Hz(hz);
        }

        public static Hz Hz(decimal hz)
        {
            return new Hz(Convert.ToDouble(hz));
        }
    }
}
