namespace Sodium
{
    using System.Collections.Generic;

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
        /// Fire the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="firing">The value to fire to registered callbacks</param>
        protected virtual bool Fire(T firing, Transaction transaction)
        {
            this.FireSubscriptionCallbacks(firing, transaction);
            return true;
        }

        /// <summary>
        /// Creates a callback that calls the Fire method on the current Event when invoked
        /// </summary>
        /// <returns>In ISodiumCallback that calls Fire, when invoked.</returns>
        protected ISodiumCallback<T> CreateFireCallback()
        {
            return new SodiumCallback<T>((t, v) => this.Fire(t, v));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firing"></param>
        /// <param name="transaction"></param>
        protected void FireSubscriptionCallbacks(T firing, Transaction transaction)
        {
            var clone = new List<ISubscription<T>>(this.Subscriptions);
            foreach (var subscription in clone)
            {
                this.FireSubscriptionCallback(firing, subscription, transaction);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="toFire"></param>
        /// <param name="transaction"></param>
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
