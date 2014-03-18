namespace Sodium
{
    /// <summary>
    /// SubscribeFireEvent is an Event that fires some initial values 
    /// when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    /// <remarks>Used by Behavior to support firing of initial values of the Behavior</remarks>
    internal abstract class FireOnSubscribeEvent<T> : RefireEvent<T>
    {
        /// <summary>
        /// Gets the values that will be sent to newly added
        /// </summary>
        /// <returns>An Array of values that will be fired to all registered subscriptions</returns>
        /// <remarks>InitialFirings is used to support initial firings of behaviors when 
        /// the underlying event is subscribed to</remarks>
        public virtual T[] SubscriptionFirings()
        {
            return null;
        }

        internal static TF[] GetSubscribeFirings<TF>(IEvent<TF> source)
        {
            var sink = source as FireOnSubscribeEvent<TF>;
            return sink == null ? null : sink.SubscriptionFirings();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="transaction"></param>
        protected override void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
        {
            this.Fire(subscription, transaction);
            base.OnSubscribe(subscription, transaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="transaction"></param>
        private void Fire(ISubscription<T> subscription, Transaction transaction)
        {
            var toFire = this.SubscriptionFirings();
            this.Fire(subscription, toFire, transaction);
        }
    }
}