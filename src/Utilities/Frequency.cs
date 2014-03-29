namespace Potassium.Utilities
{
    using System;

    /// <summary>
    /// Helper class for generating Hz instances
    /// </summary>
    public static class Frequency
    {
        /// <summary>
        /// Constructs a new Hz from the given interval
        /// </summary>
        /// <param name="ts">The interval between cycles</param>
        /// <returns>The Hz frequency for the interval</returns>
        public static Hz Hz(TimeSpan ts)
        {
            return new Hz(ts);
        }

        /// <summary>
        /// Constructs a new Hz from the given Hertz value, as a double
        /// </summary>
        /// <param name="hz">The frequency, in Hertz</param>
        /// <returns>The Hz frequency for the given Hertz value</returns>
        public static Hz Hz(double hz)
        {
            return new Hz(hz);
        }

        /// <summary>
        /// Constructs a new Hz from the given Hertz value
        /// </summary>
        /// <param name="hz">The frequency, in Hertz, as a decimal</param>
        /// <returns>The Hz frequency for the given Hertz value</returns>
        public static Hz Hz(decimal hz)
        {
            return new Hz(Convert.ToDouble(hz));
        }
    }
}
