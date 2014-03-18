namespace Sodium
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// RefireEvent is an Event that refires values that have been fired in the current
    /// Transaction when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    public abstract class RefireEvent<T> : Event<T>
    {
        /// <summary>
        /// List of values that have been fired on the current Event in the current transaction.
        /// Any subscriptions that are registered in the current transaction will get fired
        /// these values on registration.
        /// </summary>
        private readonly List<T> firings = new List<T>();

        protected override bool Fire(T firing, Transaction transaction)
        {
            this.ScheduleClearFirings(transaction);
            this.AddFiring(firing);
            this.FireSubscriptionCallbacks(firing, transaction);
            return true;
        }

        /// <summary>
        /// Anything fired already in this transaction must be re-fired now so that
        /// there's no order dependency between Fire and Subscribe.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="subscription"></param>
        internal virtual bool Refire(ISubscription<T> subscription, Transaction transaction)
        {
            var toFire = this.firings;
            this.Fire(subscription, toFire, transaction);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="transaction"></param>
        protected override void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
        {
            this.Refire(subscription, transaction);
            base.OnSubscribe(subscription, transaction);
        }

        private void ScheduleClearFirings(Transaction transaction)
        {
            var noFirings = !this.firings.Any();
            if (noFirings)
            {
                transaction.Medium(() => this.firings.Clear());
            }
        }

        private void AddFiring(T firing)
        {
            this.firings.Add(firing);
        }
    }
}