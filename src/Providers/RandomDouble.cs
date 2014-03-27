namespace Potassium.Providers
{
    using System;
    
    /// <summary>
    /// RandomDouble is a Monad that lazily returns a random double when it's value is requested
    /// </summary>
    public class RandomDouble : Provider<double>
    {
        private readonly Random rnd = new Random();
        private readonly double max;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="max"></param>
        public RandomDouble(double max)
        {
            this.max = max;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override double Value
        {
            get
            {
                return rnd.NextDouble() * max;
            }
        }
    }
}
