namespace Sodium
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// CoalesceEvent combines multiple firings from a source into a single firing, using a 
    /// combining function.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the source</typeparam>
    internal class CoalesceEvent<T> : SubscribeFireEvent<T>
    {
        private Func<T, T, T> coalesce;
        private ISubscription<T> subscription;
        private Maybe<T> accumulatedValue = Maybe<T>.Null;

        public CoalesceEvent(IObservable<T> source, Func<T, T, T> coalesce, Transaction transaction)
        {
            this.Source = source;
            this.coalesce = coalesce;

            var callback = new SodiumCallback<T>(Accumulate);
            subscription = source.Subscribe(callback, Rank, transaction);
        }

        protected IObservable<T> Source { get; private set; }

        private bool PreviouslyFired
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
            if (PreviouslyFired)
            {
                Combine(data);
            }
            else
            {
                ScheduleFiring(data, transaction);
            }
        }

        /// <summary>
        /// There was a previous firing, so combine the new firing with the old firing 
        /// using the combining function.
        /// </summary>
        /// <param name="newFiring">The newly fired value</param>
        private void Combine(T newFiring)
        {
            var newValue = coalesce(accumulatedValue.Value(), newFiring);
            accumulatedValue = new Maybe<T>(newValue);
        }

        /// <summary>
        /// There is no previous firing, so set the value to the fired value, 
        /// and schedule a firing that will happen when the transaction is closed.
        /// </summary>
        /// <param name="initialValue">The initial value</param>
        /// <param name="transaction">The current transaction</param>
        private void ScheduleFiring(T initialValue, Transaction transaction)
        {
            accumulatedValue = new Maybe<T>(initialValue);
            transaction.High(Fire, Rank);
        }

        /// <summary>
        /// Callback method that fires the accumulated value when the current
        /// transaction is closed.
        /// </summary>
        /// <param name="transaction">The current transaction</param>
        /// <remarks>The accumulated value is cleared after the firing.</remarks>
        private void Fire(Transaction transaction)
        {
            var v = accumulatedValue.Value();
            Fire(v, transaction);
            accumulatedValue = Maybe<T>.Null;
        }

        /// <summary>
        /// Combine the list of values to fire during subscription using the combining function
        /// </summary>
        /// <param name="firings">The firings to combine</param>
        /// <returns>The combined value</returns>
        private T Combine(IList<T> firings)
        {
            var e = firings[0];
            for (var i = 1; i < firings.Count; i++)
            {
                e = coalesce(e, firings[i]);
            }

            return e;
        }
    }
}