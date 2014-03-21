namespace JT.Rx.Net.Continuous
{
    using JT.Rx.Net.Core;
    using System;

    public class RandomIntBehavior : Monad<int>
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