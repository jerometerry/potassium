namespace JT.Rx.Net.Internal
{
    
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// CoalesceEvent combines multiple publishings from a source into a single publishing, using a 
    /// combining function.
    /// </summary>
    /// <typeparam name="T">The type of values published through the source</typeparam>
    internal class CoalesceEvent<T> : SubscribePublishEvent<T>
    {
        private Func<T, T, T> coalesce;
        private ISubscription<T> subscription;
        private Maybe<T> accumulatedValue = Maybe<T>.Null;

        public CoalesceEvent(Observable<T> source, Func<T, T, T> coalesce, Transaction transaction)
        {
            this.Source = source;
            this.coalesce = coalesce;

            var callback = new SubscriptionPublisher<T>(Accumulate);
            subscription = source.CreateSubscription(callback, Priority, transaction);
        }

        protected Observable<T> Source { get; private set; }

        private bool PreviouslyPublished
        {
            get
            {
                return this.accumulatedValue.HasValue;
            }
        }

        public override T[] SubscriptionFirings()
        {
            var events = GetSubscribeFirings(Source);
            if (events == null)
            {
                return null;
            }

            var e = Combine(events);
            return new[] { e };
        }

        protected override void Dispose(bool disposing)
        {
            if (subscription != null)
            {
                subscription.Dispose();
                subscription = null;
            }

            Source = null;
            coalesce = null;

            base.Dispose(disposing);
        }

        private void Accumulate(T data, Transaction transaction)
        {
            if (this.PreviouslyPublished)
            {
                Combine(data);
            }
            else
            {
                ScheduleFiring(data, transaction);
            }
        }

        /// <summary>
        /// There was a previous publishing, so combine the new publishing with the old publishing 
        /// using the combining function.
        /// </summary>
        /// <param name="newFiring">The newly published value</param>
        private void Combine(T newFiring)
        {
            var newValue = coalesce(accumulatedValue.Value, newFiring);
            accumulatedValue = new Maybe<T>(newValue);
        }

        /// <summary>
        /// There is no previous publishing, so set the value to the published value, 
        /// and schedule a publishing that will happen when the transaction is closed.
        /// </summary>
        /// <param name="initialValue">The initial value</param>
        /// <param name="transaction">The current transaction</param>
        private void ScheduleFiring(T initialValue, Transaction transaction)
        {
            accumulatedValue = new Maybe<T>(initialValue);
            transaction.High(Publish, Priority);
        }

        /// <summary>
        /// Callback method that publishes the accumulated value when the current
        /// transaction is closed.
        /// </summary>
        /// <param name="transaction">The current transaction</param>
        /// <remarks>The accumulated value is cleared after the publishing.</remarks>
        private void Publish(Transaction transaction)
        {
            var v = accumulatedValue.Value;
            this.Publish(v, transaction);
            accumulatedValue = Maybe<T>.Null;
        }

        /// <summary>
        /// Combine the list of values to publish during subscription using the combining function
        /// </summary>
        /// <param name="publishings">The publishings to combine</param>
        /// <returns>The combined value</returns>
        private T Combine(IList<T> publishings)
        {
            var e = publishings[0];
            for (var i = 1; i < publishings.Count; i++)
            {
                e = coalesce(e, publishings[i]);
            }

            return e;
        }
    }
}