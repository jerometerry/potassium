﻿namespace Sodium
{
    using System.Collections.Generic;

    /// <summary>
    /// An EventSink is an Event that can be Fired.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    public class EventSink<T> : Event<T>
    {
        /// <summary>
        /// Fire the given value to all registered subscriptions
        /// </summary>
        /// <param name="firing">The value to be fired</param>
        /// <returns>True if the fire was successful, false otherwise.</returns>
        public bool Fire(T firing)
        {
            return this.StartTransaction(t => this.Fire(firing, t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="values"></param>
        /// <param name="transaction"></param>
        protected static void NotifySubscriber(ISubscription<T> subscription, ICollection<T> values, Transaction transaction)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            foreach (var value in values)
            {
                NotifySubscriber(value, subscription, transaction);
            }
        }

        protected static void NotifySubscriber(T value, ISubscription<T> subscription, Transaction transaction)
        {
            subscription.Notification.Send(value, transaction);
        }

        /// <summary>
        /// Fire the given value to all subscribers
        /// </summary>
        /// <param name="value">The value to fire</param>
        /// <param name="transaction">The current transaction</param>
        protected void NotifySubscribers(T value, Transaction transaction)
        {
            var clone = this.Subscriptions.ToArray();
            foreach (var subscription in clone)
            {
                NotifySubscriber(value, subscription, transaction);
            }
        }

        /// <summary>
        /// Fire the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="firing">The value to fire to registered callbacks</param>
        protected virtual bool Fire(T firing, Transaction transaction)
        {
            this.NotifySubscribers(firing, transaction);
            return true;
        }

        /// <summary>
        /// Creates a callback that calls the Fire method on the current Event when invoked
        /// </summary>
        /// <returns>In INotification that calls Fire, when invoked.</returns>
        protected INotification<T> CreateFireCallback()
        {
            return new Notification<T>((t, v) => this.Fire(t, v));
        }
    }
}
