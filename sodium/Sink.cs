namespace Sodium
{
    using System.Collections.Generic;

    /// <summary>
    /// An EventSink is an Event that you can fire updates through
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the Event.</typeparam>
    public class Sink<T> : Event<T>, IFireable<T>
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
        /// Fire the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="firing">The value to fire to registered callbacks</param>
        internal virtual bool Fire(T firing, Transaction transaction)
        {
            this.FireSubscriptionCallbacks(firing, transaction);
            return true;
        }

        /// <summary>
        /// Creates a callback that calls the Fire method on the current Event when invoked
        /// </summary>
        /// <returns>In ISodiumCallback that calls Fire, when invoked.</returns>
        internal ISodiumCallback<T> CreateFireCallback()
        {
            return new SodiumCallback<T>((t, v) => this.Fire(t, v));
        }

        internal void FireSubscriptionCallbacks(T firing, Transaction transaction)
        {
            var clone = new List<ISubscription<T>>(this.Subscriptions);
            foreach (var subscription in clone)
            {
                this.FireSubscriptionCallback(firing, subscription, transaction);
            }
        }

        protected void Fire(ISubscription<T> subscription, ICollection<T> toFire, Transaction transaction)
        {
            if (toFire == null || toFire.Count == 0)
            {
                return;
            }

            foreach (var firing in toFire)
            {
                this.FireSubscriptionCallback(firing, subscription, transaction);
            }
        }

        private void FireSubscriptionCallback(T firing, ISubscription<T> subscription, Transaction transaction)
        {
            var l = (Subscription<T>)subscription;
            l.Callback.Fire(firing, subscription, transaction);
        }
    }
}
