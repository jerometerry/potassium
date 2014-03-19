namespace Sodium
{
    using System.Collections.Generic;

    /// <summary>
    /// An EventPublisher is an Event that allows callers to publish values to subscribers.
    /// </summary>
    /// <typeparam name="T">The type of values published through the Event</typeparam>
    public class EventPublisher<T> : Event<T>
    {
        /// <summary>
        /// Publish the given value to all registered subscriptions
        /// </summary>
        /// <param name="value">The value to be published</param>
        /// <returns>True if the publish was successful, false otherwise.</returns>
        public bool Publish(T value)
        {
            return this.StartTransaction(t => this.Publish(value, t));
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
        /// Publish the given value to all subscribers
        /// </summary>
        /// <param name="value">The value to publish</param>
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
        /// Publish the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="value">The value to publish to registered callbacks</param>
        protected virtual bool Publish(T value, Transaction transaction)
        {
            this.NotifySubscribers(value, transaction);
            return true;
        }

        /// <summary>
        /// Creates a callback that calls the Publish method on the current Event when invoked
        /// </summary>
        /// <returns>In INotification that calls Publish, when invoked.</returns>
        protected INotification<T> CreatePublishCallback()
        {
            return new Notification<T>((t, v) => this.Publish(t, v));
        }
    }
}
