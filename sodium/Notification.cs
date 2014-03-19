namespace Sodium
{
    using System;

    /// <summary>
    /// ActionCallback wraps an System.Action used to subscribe to Observables.
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired</typeparam>
    public sealed class Notification<T> : INotification<T>
    {
        private readonly Action<T, Transaction> action;

        /// <summary>
        /// Constructs a new Notification from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable fires</param>
        public Notification(Action<T, Transaction> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        /// <param name="value">The value to be fired to the </param>
        /// <param name="transaction">The Transaction used to order the firing</param>
        public void Send(T value, Transaction transaction)
        {
            action(value, transaction);
        }
    }
}