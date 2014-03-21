namespace Sodium.Continuous
{
    using System;

    public class RandomIntBehavior : ContinuousBehavior<int>
    {
        private Random rnd = new Random();
        private int max;

        public RandomIntBehavior(int max)
        {
            this.max = max;
        }

        public override int Value
        {
            get
            {
                return this.rnd.Next(this.max);
            }
        }
    }
}