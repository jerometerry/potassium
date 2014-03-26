namespace Potassium.Internal
{
    using Potassium.Core;    

    /// <summary>
    /// FireEvent is an Event that fires some initial values 
    /// when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    /// <remarks>Used by Behavior to support firing of initial values of the Behavior</remarks>
    internal abstract class FireEvent<T> : RefireEvent<T>
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

        internal static TF[] GetSubscribeFirings<TF>(Observable<TF> source)
        {
            var sink = source as FireEvent<TF>;
            return sink == null ? null : sink.SubscriptionFirings();
        }

        /// <summary>
        /// Fires the initial subscription firings to the newly created subscription
        /// </summary>
        /// <param name="subscription">The newly created subscription</param>
        /// <param name="transaction">The current transaction</param>
        internal override void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
        {
            var values = this.SubscriptionFirings();
            Observer<T>.Notify(subscription, values, transaction);
            base.OnSubscribe(subscription, transaction);
        }
    }
}