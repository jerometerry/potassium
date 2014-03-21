namespace Sodium.Continuous
{
    using System;

    public class RandomDoubleBehavior : ContinuousBehavior<double>
    {
        private Random rnd = new Random();
        private double max;

        public RandomDoubleBehavior(double max)
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
