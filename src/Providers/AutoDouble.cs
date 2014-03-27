namespace Potassium.Providers
{
    /// <summary>
    /// AutoDouble is an IProvider of type double that starts with an initial value, 
    /// and auto increments by a step after each request of the Value
    /// </summary>
    public class AutoDouble : IProvider<double>
    {
        private readonly double increment;
        private double value;

        /// <summary>
        /// Constructs a new AutoDouble, starting at value 0.0, with increment of 1.0
        /// </summary>
        public AutoDouble()
            : this(0.0, 1.0)
        {
        }

        /// <summary>
        /// Constructs a new AutoDouble, starting at the given value, with an increment of 1.0
        /// </summary>
        /// <param name="value"></param>
        public AutoDouble(double value)
            : this(value, 1.0)
        {
        }

        /// <summary>
        /// Constructs a new AutoDouble, with the given start and increment
        /// </summary>
        /// <param name="value">The starting value</param>
        /// <param name="increment">The amount to increment the value by when the Value property is requested</param>
        public AutoDouble(double value, double increment)
        {
            this.value = value;
            this.increment = increment;
        }

        /// <summary>
        /// Gets the current value, then increment.
        /// </summary>
        public double Value
        {
            get
            {
                var result = value;
                value += increment;
                return result;
            }
        }
    }
}
