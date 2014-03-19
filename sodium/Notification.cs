namespace Sodium
{
    using System;

    /// <summary>
    /// Notification wraps an System.Action used to subscribe to Observables.
    /// </summary>
    /// <typeparam name="T">The type of value that will be published</typeparam>
    internal sealed class Notification<T> : INotification<T>
    {
        private readonly Action<T, Transaction> action;

        /// <summary>
        /// Constructs a new Notification from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable publishes</param>
        public Notification(Action<T> action)
            : this((a, t) => action(a))
        {
        }

        /// <summary>
        /// Constructs a new Notification from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable publishes</param>
        public Notification(Action<T, Transaction> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        /// <param name="value">The value to be published to the </param>
        /// <param name="transaction">The Transaction used to order the publishing</param>
        public void Send(T value, Transaction transaction)
        {
            action(value, transaction);
        }
    }
}