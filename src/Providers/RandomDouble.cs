namespace Potassium.Providers
{
    using System;
    
    /// <summary>
    /// RandomDouble is a Monad that lazily returns a random double when it's value is requested
    /// </summary>
    public class RandomDouble : Provider<double>
    {
        private Random rnd = new Random();
        private double max;

        public RandomDouble(double max)
        {
            this.max = max;
        }

        public override double Value
        {
            get
            {
                return rnd.NextDouble() * max;
            }
        }
    }
}
