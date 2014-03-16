namespace Sodium
{
    /// <summary>
    /// InitialFireEventSink is an EventSink that is fired some initial values 
    /// when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    /// <remarks>Used by Behavior to support firing of initial values of the Behavior</remarks>
    public class InitialFireEventSink<T> : RefireEventSink<T>
    {
        internal static TF[] GetInitialFirings<TF>(Event<TF> source)
        {
            var sink = source as InitialFireEventSink<TF>;
            if (sink == null)
            {
                return null;
            }

            return sink.InitialFirings();
        }

        /// <summary>
        /// Gets the values that will be sent to newly added
        /// </summary>
        /// <returns>An Array of values that will be fired to all registered subscriptions</returns>
        /// <remarks>InitialFirings is used to support initial firings of behaviors when 
        /// the underlying event is subscribed to</remarks>
        protected internal virtual T[] InitialFirings()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="transaction"></param>
        protected override void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
        {
            this.InitialFire(subscription, transaction);
            base.OnSubscribe(subscription, transaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="transaction"></param>
        private void InitialFire(ISubscription<T> subscription, Transaction transaction)
        {
            var toFire = this.InitialFirings();
            this.Fire(subscription, toFire, transaction);
        }
    }
}