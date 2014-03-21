namespace JT.Rx.Net.Continuous
{
    using JT.Rx.Net.Core;
    using System;

    public class RandomDoubleBehavior : Monad<double>
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
