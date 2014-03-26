namespace Potassium.Internal
{
    using System;
    using System.Collections.Generic;
    using Potassium.Core;

    /// <summary>
    /// The Observer in the Observer Pattern, used to notify subscribers of new values.
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired</typeparam>
    internal sealed class Observer<T>
    {
        private readonly Action<T, Transaction> action;

        /// <summary>
        /// Constructs a new Notification from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable fires</param>
        public Observer(Action<T> action)
            : this((a, t) => action(a))
        {
        }

        /// <summary>
        /// Constructs a new Notification from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable fires</param>
        public Observer(Action<T, Transaction> action)
        {
            this.action = action;
        }

        public static void Notify(T value, ISubscription<T>[] subscriptions, Transaction transaction)
        {
            foreach (var subscription in subscriptions)
            {
                Notify(value, subscription, transaction);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="values"></param>
        /// <param name="transaction"></param>
        public static void Notify(ISubscription<T> subscription, ICollection<T> values, Transaction transaction)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            foreach (var value in values)
            {
                Notify(value, subscription, transaction);
            }
        }

        public static void Notify(T value, ISubscription<T> subscription, Transaction transaction)
        {
            var s = (Subscription<T>)subscription;
            s.Observer.Notify(value, transaction);
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        /// <param name="value">The value to be fired to the </param>
        /// <param name="transaction">The Transaction used to order the firing</param>
        public void Notify(T value, Transaction transaction)
        {
            action(value, transaction);
        }
    }
}