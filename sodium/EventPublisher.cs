namespace Sodium
{
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
        /// Creates a callback that calls the Publish method on the current Event when invoked
        /// </summary>
        /// <returns>In Publisher that calls Publish, when invoked.</returns>
        internal Publisher<T> CreatePublisher()
        {
            return new Publisher<T>((t, v) => this.Publish(t, v));
        }

        /// <summary>
        /// Publish the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="value">The value to publish to registered callbacks</param>
        protected virtual bool Publish(T value, Transaction transaction)
        {
            var clone = this.Subscriptions.ToArray();
            Publisher<T>.PublishToSubscribers(value, clone, transaction);
            return true;
        }
    }
}
