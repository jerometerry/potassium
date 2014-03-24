namespace Potassium.Core
{
    using System;

    /// <summary>
    /// The frequency, in Hertz, of a Signal
    /// </summary>
    public class Hz
    {
        private static readonly  Hz max = new Hz(1000);
        
        public static Hz Max
        {
            get
            {
                return max;
            }
        }
        
        public Hz(double hz)
        {
            this.Value = hz;
        }

        public Hz(TimeSpan interval)
        {
            this.Value = 1 / interval.TotalSeconds;
        }

        public double Value { get; set; }

        public long Milliseconds
        {
            get
            {
                return Convert.ToInt64(1000.0 / this.Value);
            }
        }

        public TimeSpan Interval()
        {
            return TimeSpan.FromMilliseconds(1000 / this.Value);
        }
    }
}
