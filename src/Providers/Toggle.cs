namespace Potassium.Providers
{
    using System;
    using System.Threading;
    using Potassium.Internal;

    /// <summary>
    /// Toggle is an IProvider of type bool that starts with an initial value, 
    /// and flips it's value after each request to the Value property.
    /// </summary>
    public class Toggle : Provider<bool>
    {
        private bool value;

        /// <summary>
        /// Constructs a new Toggle, initialized to false.
        /// </summary>
        public Toggle()
            : this(false)
        {
        }

        /// <summary>
        /// Constructs a new Toggle having the specified initial value
        /// </summary>
        /// <param name="initValue">The initial value for the Toggle</param>
        public Toggle(bool initValue)
        {
            value = initValue;
        }

        /// <summary>
        /// Get and toggle the current (bool) value.
        /// </summary>
        public override bool Value
        {
            get
            {
                if (Monitor.TryEnter(Constants.ProviderValueLock, Constants.LockTimeout))
                {
                    try
                    {
                        var result = value;
                        value = !value;
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
