namespace Sodium
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// SubscribeRepublishEvent is an Event that republishes values that have been published in the current
    /// Transaction when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values published through the Event</typeparam>
    public abstract class SubscribeRepublishEvent<T> : EventPublisher<T>
    {
        /// <summary>
        /// List of values that have been published on the current Event in the current transaction.
        /// Any subscriptions that are registered in the current transaction will get published
        /// these values on registration.
        /// </summary>
        private readonly List<T> publishings = new List<T>();

        /// <summary>
        /// Publish the given value to all registered callbacks, and stores the publishing 
        /// to be republished to any subscribers in the same transaction.
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="value">The value to publish to registered callbacks</param>
        /// <remarks>Overrides EventPublisher.Publish</remarks>
        protected override bool Publish(T value, Transaction transaction)
        {
            this.ScheduleClearPublishings(transaction);
            this.RecordPublishing(value);
            this.NotifySubscribers(value, transaction);
            return true;
        }

        /// <summary>
        /// Anything published already in this transaction must be re-published now so that
        /// there's no order dependency between Publish and Subscribe.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="subscription"></param>
        protected virtual bool Republish(ISubscription<T> subscription, Transaction transaction)
        {
            var values = this.publishings;
            NotifySubscriber(subscription, values, transaction);
            return true;
        }

        /// <summary>
        /// Republishes any values published in the current transaction to the given subscription
        /// </summary>
        /// <param name="subscription">The newly created subscription</param>
        /// <param name="transaction">The current transaction</param>
        protected override void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
        {
            this.Republish(subscription, transaction);
            base.OnSubscribe(subscription, transaction);
        }

        private void ScheduleClearPublishings(Transaction transaction)
        {
            var noFirings = !this.publishings.Any();
            if (noFirings)
            {
                transaction.Medium(() => this.publishings.Clear());
            }
        }

        private void RecordPublishing(T value)
        {
            this.publishings.Add(value);
        }
    }
}