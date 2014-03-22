namespace JT.Rx.Net
{
    using JT.Rx.Net.Internal;    

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
            return Transaction.Start(t => this.Publish(value, t));
        }

        /// <summary>
        /// Creates a SubscriptionPublisher that can notify subscribers to the current Event of new values.
        /// </summary>
        /// <returns>In Publisher that calls Publish, when invoked.</returns>
        /// <remarks>The returned SubscriptionPublisher can be used by the current Event to notify subscribers
        /// on the Event they were subscribed on, or by another event to forward values to subscribers from an
        /// alternate Event.</remarks>
        internal SubscriptionPublisher<T> CreateSubscriptionPublisher()
        {
            return new SubscriptionPublisher<T>((t, v) => this.Publish(t, v));
        }

        /// <summary>
        /// Publish the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="value">The value to publish to registered callbacks</param>
        protected virtual bool Publish(T value, Transaction transaction)
        {
            var clone = this.Subscriptions.ToArray();
            SubscriptionPublisher<T>.PublishToSubscribers(value, clone, transaction);
            return true;
        }
    }
}
