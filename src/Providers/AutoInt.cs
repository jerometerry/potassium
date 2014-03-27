namespace Potassium.Providers
{
    using System;
    using System.Threading;
    using Potassium.Internal;

    /// <summary>
    /// AutoDouble is an IProvider of type int that starts with an initial value, 
    /// and auto increments by a step after each request of the Value
    /// </summary>
    public class AutoInt : IProvider<int>
    {
        private readonly int increment;
        private int value;

        /// <summary>
        /// Constructs a new AutoInt, starting at 0, with increment of 1
        /// </summary>
        public AutoInt()
            : this(0, 1)
        {
        }

        /// <summary>
        /// Constructs a new AutoInt, starting at the given value, with an increment of 1
        /// </summary>
        /// <param name="value"></param>
        public AutoInt(int value)
            : this(value, 1)
        {
        }

        /// <summary>
        /// Constructs a new AutoInt, starting at the given value, having the given increment
        /// </summary>
        /// <param name="value">The initial value of the AutoInt</param>
        /// <param name="increment">The amount to increment the value by after each request of the Value property</param>
        public AutoInt(int value, int increment)
        {
            this.value = value;
            this.increment = increment;
        }

        /// <summary>
        /// Get and increment the current (int) value.
        /// </summary>
        public int Value
        {
            get
            {
                if (Monitor.TryEnter(Constants.ProviderValueLock, Constants.LockTimeout))
                { 
                    try
                    { 
                        var result = value;
                        value += increment;
                        return result;
                    }
                    finally
                    {
                        Monitor.Exit(Constants.ProviderValueLock);
                    }
                }

                throw new InvalidOperationException("Could not obtain the provider value lock");
            }
        }
    }
}
