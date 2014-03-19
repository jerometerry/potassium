namespace Sodium
{
    /// <summary>
    /// SubscribePublishEvent is an Event that publishes some initial values 
    /// when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values published through the Event</typeparam>
    /// <remarks>Used by Behavior to support publishing of initial values of the Behavior</remarks>
    internal abstract class SubscribePublishEvent<T> : SubscribeRepublishEvent<T>
    {
        /// <summary>
        /// Gets the values that will be sent to newly added
        /// </summary>
        /// <returns>An Array of values that will be published to all registered subscriptions</returns>
        /// <remarks>InitialFirings is used to support initial publishings of behaviors when 
        /// the underlying event is subscribed to</remarks>
        public virtual T[] SubscriptionFirings()
        {
            return null;
        }

        internal static TF[] GetSubscribeFirings<TF>(Observable<TF> source)
        {
            var sink = source as SubscribePublishEvent<TF>;
            return sink == null ? null : sink.SubscriptionFirings();
        }

        /// <summary>
        /// Publishes the initial subscription publishings to the newly created subscription
        /// </summary>
        /// <param name="subscription">The newly created subscription</param>
        /// <param name="transaction">The current transaction</param>
        protected override void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
        {
            var values = this.SubscriptionFirings();
            PublishToSubscriber(subscription, values, transaction);
            base.OnSubscribe(subscription, transaction);
        }
    }
}