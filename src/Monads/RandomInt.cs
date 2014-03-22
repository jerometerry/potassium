namespace JT.Rx.Net.Monads
{
    using System;

    /// <summary>
    /// RandomInt is a Monad that lazily returns a random int when it's value is requested
    /// </summary>
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