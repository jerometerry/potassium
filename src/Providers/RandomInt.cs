namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// RandomInt is a Monad that lazily returns a random int when it's value is requested
    /// </summary>
    public class RandomInt : Provider<int>
    {
        private readonly Random rnd = new Random();

        /// <summary>
        /// Constructs a new RandomInt, that generates values between zero and the maximum value
        /// </summary>
        /// <param name="max">The maximum value</param>
        public RandomInt(int max)
        {
            this.Max = max;
        }

        /// <summary>
        /// Gets the maximum value
        /// </summary>
        public int Max { get; private set; }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override int Value
        {
            get
            {
                return this.rnd.Next(this.Max);
            }
        }
    }
}