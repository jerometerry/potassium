namespace JT.Rx.Net.Continuous
{
    using System;
    using JT.Rx.Net.Core;

    public class RandomDouble : Monad<double>
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
