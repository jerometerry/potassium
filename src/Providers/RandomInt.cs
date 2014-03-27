namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// RandomInt is a Monad that lazily returns a random int when it's value is requested
    /// </summary>
    public class RandomInt : Provider<int>
    {
        private readonly Random rnd = new Random();
        private readonly int max;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="max"></param>
        public RandomInt(int max)
        {
            this.max = max;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override int Value
        {
            get
            {
                return this.rnd.Next(this.max);
            }
        }
    }
}