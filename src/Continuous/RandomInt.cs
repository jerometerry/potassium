namespace JT.Rx.Net.Continuous
{
    using System;
    using JT.Rx.Net.Core;

    public class RandomInt : Monad<int>
    {
        private Random rnd = new Random();
        private int max;

        public RandomInt(int max)
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