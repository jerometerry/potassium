namespace Potassium.Core
{
    using System;

    /// <summary>
    /// The frequency, in Hertz, of a Signal
    /// </summary>
    public class Hz
    {
        public double Value { get; set; }

        public Hz(double hz)
        {
            this.Value = hz;
        }

        public Hz(TimeSpan interval)
        {
            this.Value = 1 / interval.TotalSeconds;
        }

        public TimeSpan Interval()
        {
            return TimeSpan.FromSeconds(1 / this.Value);
        }
    }
}
