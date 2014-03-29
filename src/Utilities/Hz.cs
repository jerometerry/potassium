namespace Potassium.Utilities
{
    using System;

    /// <summary>
    /// The frequency, in Hertz, of a Signal
    /// </summary>
    public class Hz
    {
        /// <summary>
        /// Gets the maximum value of Hz (1000 Hz).
        /// </summary>
        public static readonly Hz Max = new Hz(1000);
        
        /// <summary>
        /// Constructs a new Hz, with the given frequency in Hertz
        /// </summary>
        /// <param name="hz"></param>
        public Hz(double hz)
        {
            this.Value = hz;
        }

        /// <summary>
        /// Constructs a new Hz, with the given interval between cycles
        /// </summary>
        /// <param name="interval">The time span between cycles</param>
        public Hz(TimeSpan interval)
        {
            this.Value = 1 / interval.TotalSeconds;
        }

        /// <summary>
        /// Gets / Sets the frequency, in Hertz
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets the number of milliseconds between cycles
        /// </summary>
        public long Milliseconds
        {
            get
            {
                return Convert.ToInt64(1000.0 / this.Value);
            }
        }

        /// <summary>
        /// Gets the timespan between cycles
        /// </summary>
        /// <returns></returns>
        public TimeSpan Interval()
        {
            return TimeSpan.FromMilliseconds(1000 / this.Value);
        }
    }
}
