namespace Potassium.Providers
{
    using System;
    
    /// <summary>
    /// RandomDouble is a Monad that lazily returns a random double when it's value is requested
    /// </summary>
    public class RandomDouble : Provider<double>
    {
        private readonly Random rnd = new Random();

        /// <summary>
        /// Constructs a new RandomDouble, which prodoces value between zero and the maximum value
        /// </summary>
        /// <param name="max">The maximum value</param>
        public RandomDouble(double max)
        {
            this.Max = max;
        }

        /// <summary>
        /// Gets the maximum value
        /// </summary>
        public double Max { get; private set; }

        /// <summary>
        /// Returns a random double between zero and the maximum value
        /// </summary>
        public override double Value
        {
            get
            {
                return rnd.NextDouble() * this.Max;
            }
        }
    }
}
