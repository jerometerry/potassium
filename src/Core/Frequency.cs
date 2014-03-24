namespace Potassium.Core
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

        public static Hz kHz(double khz)
        {
            return new Hz(1000.0 * khz);
        }

        public static Hz MHz(double mhz)
        {
            return new Hz(1000000.0 * mhz);
        }
    }
}
