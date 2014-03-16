namespace Sodium
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An EventSink is an Event that you can fire updates through
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the Event.</typeparam>
    public class EventSink<T> : Event<T>
    {
        /// <summary>
        /// List of values that have been fired on the current Event in the current transaction.
        /// Any subscriptions that are registered in the current transaction will get fired
        /// these values on registration.
        /// </summary>
        private readonly List<T> firings = new List<T>();

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
            ScheduleClearFirings(transaction);
            AddFiring(firing);
            this.FireSubscriptionCallbacks(firing, transaction);
            return true;
        }

        /// <summary>
        /// Creates a callback that calls the Fire method on the current Event when invoked
        /// </summary>
        /// <returns>In ISodiumCallback that calls Fire, when invoked.</returns>
        internal ISodiumCallback<T> CreateFireCallback()
        {
            return new ActionCallback<T>((t, v) => this.Fire(t, v));
        }

        /// <summary>
        /// Anything fired already in this transaction must be re-fired now so that
        /// there's no order dependency between Fire and Subscribe.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="subscription"></param>
        internal virtual bool Refire(ISubscription<T> subscription, Transaction transaction)
        {
            var toFire = firings;
            this.Fire(subscription, toFire, transaction);
            return true;
        }

        internal override ISubscription<T> CreateSubscription(ISodiumCallback<T> source, Rank superior, Transaction transaction)
        {
            var subscription = base.CreateSubscription(source, superior, transaction);
            Refire(subscription, transaction);
            return subscription;
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

        private void FireSubscriptionCallbacks(T firing, Transaction transaction)
        {
            var clone = new List<ISubscription<T>>(this.Subscriptions);
            foreach (var subscription in clone)
            {
                this.FireSubscriptionCallback(firing, subscription, transaction);
            }
        }

        private void FireSubscriptionCallback(T firing, ISubscription<T> subscription, Transaction transaction)
        {
            var l = (Subscription<T>)subscription;
            l.Callback.Fire(firing, subscription, transaction);
        }

        private void ScheduleClearFirings(Transaction transaction)
        {
            var noFirings = !firings.Any();
            if (noFirings)
            {
                transaction.Medium(() => firings.Clear());
            }
        }

        private void AddFiring(T firing)
        {
            firings.Add(firing);
        }
    }
}
