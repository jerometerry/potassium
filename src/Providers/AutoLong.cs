namespace Potassium.Providers
{
    using System;
    using System.Threading;
    using Potassium.Internal;

    /// <summary>
    /// AutoLong is an IProvider of type long that starts with an initial value, 
    /// and auto increments by a step after each request of the Value
    /// </summary>
    public class AutoLong : IProvider<long>
    {
        private readonly int increment;
        private long value;

        /// <summary>
        /// 
        /// </summary>
        public AutoLong()
            : this(0, 1)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public AutoLong(long value)
            : this(value, 1)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="increment"></param>
        public AutoLong(long value, int increment)
        {
            this.value = value;
            this.increment = increment;
        }

        /// <summary>
        /// Get and increment the current (long) value.
        /// </summary>
        public long Value
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
