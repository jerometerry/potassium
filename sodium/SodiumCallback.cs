namespace Sodium
{
    using System;

    /// <summary>
    /// ActionCallback wraps an System.Action used to subscribe to Observables.
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired</typeparam>
    internal sealed class SodiumCallback<T> : ISodiumCallback<T>
    {
        private readonly Action<T, Transaction> action;

        /// <summary>
        /// Constructs a new SodiumCallback from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable fires</param>
        public SodiumCallback(Action<T, Transaction> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        /// <param name="value">The value to be fired to the </param>
        /// <param name="subscription">The subscription that holds the current callback</param>
        /// <param name="transaction">The Transaction used to order the firing</param>
        public void Invoke(T value, ISubscription<T> subscription, Transaction transaction)
        {
            action(value, transaction);
        }
    }
}