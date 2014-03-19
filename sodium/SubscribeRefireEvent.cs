namespace Sodium
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// SubscribeRefireEvent is an Event that refires values that have been fired in the current
    /// Transaction when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    public abstract class SubscribeRefireEvent<T> : EventSink<T>
    {
        /// <summary>
        /// List of values that have been fired on the current Event in the current transaction.
        /// Any subscriptions that are registered in the current transaction will get fired
        /// these values on registration.
        /// </summary>
        private readonly List<T> firings = new List<T>();

        /// <summary>
        /// Fire the given value to all registered callbacks, and stores the firing 
        /// to be refired to any subscribers in the same transaction.
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="firing">The value to fire to registered callbacks</param>
        /// <remarks>Overrides EventSink.Fire</remarks>
        protected override bool Fire(T firing, Transaction transaction)
        {
            this.ScheduleClearFirings(transaction);
            this.AddFiring(firing);
            this.NotifySubscribers(firing, transaction);
            return true;
        }

        /// <summary>
        /// Anything fired already in this transaction must be re-fired now so that
        /// there's no order dependency between Fire and Subscribe.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="subscription"></param>
        protected virtual bool Refire(ISubscription<T> subscription, Transaction transaction)
        {
            var values = this.firings;
            NotifySubscriber(subscription, values, transaction);
            return true;
        }

        /// <summary>
        /// Refires any values fired in the current transaction to the given subscription
        /// </summary>
        /// <param name="subscription">The newly created subscription</param>
        /// <param name="transaction">The current transaction</param>
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