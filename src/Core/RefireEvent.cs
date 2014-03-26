namespace Potassium.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using Potassium.Internal;

    /// <summary>
    /// RefireEvent is an Event that refires values that have been fired in the current
    /// Transaction when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    public abstract class RefireEvent<T> : FirableEvent<T>
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
        /// <param name="value">The value to fire to registered callbacks</param>
        /// <remarks>Overrides FirableEvent.Fire</remarks>
        internal override bool Fire(T value, Transaction transaction)
        {
            this.ScheduleClearFirings(transaction);
            this.RecordFiring(value);
            return base.Fire(value, transaction);
        }

        /// <summary>
        /// Anything fired already in this transaction must be re-fired now so that
        /// there's no order dependency between Fire and Subscribe.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="subscription"></param>
        internal virtual bool Refire(ISubscription<T> subscription, Transaction transaction)
        {
            var values = this.firings;
            Observer<T>.Notify(subscription, values, transaction);
            return true;
        }

        /// <summary>
        /// Refires any values fired in the current transaction to the given subscription
        /// </summary>
        /// <param name="subscription">The newly created subscription</param>
        /// <param name="transaction">The current transaction</param>
        internal override void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
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

        private void RecordFiring(T value)
        {
            this.firings.Add(value);
        }
    }
}