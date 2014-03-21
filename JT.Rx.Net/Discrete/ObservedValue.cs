namespace JT.Rx.Net.Discrete
{
    using JT.Rx.Net.Core;

    /// <summary>
    /// ObservedValue stores the last published value of an Observable,
    /// starting with an initial value.
    /// </summary>
    /// <typeparam name="T">The type of value published through the Observable</typeparam>
    public sealed class ObservedValue<T> : DisposableObject
    {
        private Observable<T> source;

        /// <summary>
        /// Holding tank for updates from the underlying Observable, waiting to be 
        /// moved into the current value.
        /// </summary>
        private Maybe<T> update = Maybe<T>.Null;

        /// <summary>
        /// Subscription that listens for publishings from the underlying Event.
        /// </summary>
        private ISubscription<T> subscription;

        /// <summary>
        /// Constructs a constant ObservedValue, having the given value.
        /// </summary>
        /// <param name="initValue">The initial value of the ObservedValue</param>
        public ObservedValue(T initValue)
        {
            this.Value = initValue;
        }

        /// <summary>
        /// Constructs a time varying ObservedValue, having an initial value that 
        /// gets updated when the Observable publishes new values.
        /// </summary>
        /// <param name="source">The Observable to monitor for value updates</param>
        /// <param name="initValue">The initial value of the ObservedValue</param>
        public ObservedValue(Observable<T> source, T initValue)
        {
            this.source = source;
            this.Value = initValue;
            this.subscription = this.Subscribe();
        }

        /// <summary>
        /// Sample the behavior's current value.
        /// </summary>
        /// <remarks>
        /// This should generally be avoided in favor of GetValueStream().Subscribe(..) so you don't
        /// miss any updates, but in many circumstances it makes sense.
        ///
        /// It can be best to use it inside an explicit transaction (using TransactionContext.Current.Run()).
        /// For example, a b.Value inside an explicit transaction along with a
        /// b.Source.Subscribe(..) will capture the current value and any updates without risk
        /// of missing any in between.
        /// </remarks>
        public T Value { get; set; }

        /// <summary>
        /// Gets the updated value of the Behavior that has not yet been moved to the
        /// current value of the Behavior. 
        /// </summary>
        /// <returns>
        /// The updated value, if any, otherwise the current value
        /// </returns>
        /// <remarks>As the underlying event is published, the current behavior is 
        /// updated with the value of the publishing. However, it doesn't go directly to the
        /// value field. Instead, the value is put into newValue, and a Last Action is
        /// scheduled to move the value from newValue to value.</remarks>
        public T NewValue
        {
            get
            {
                return this.update.HasValue ? this.update.Value() : this.Value;
            }
        }

        /// <summary>
        /// Store the updated value, and schedule a Transaction.Last action that will move the updated value 
        /// into the current value.
        /// </summary>
        /// <param name="transaction">The transaction to schedule the value update on</param>
        /// <param name="newValue">The updated value</param>
        public void ScheduleApplyValueUpdate(T newValue, Transaction transaction)
        {
            if (!this.update.HasValue)
            {
                transaction.Medium(this.ApplyValueUpdate);
            }

            this.update = new Maybe<T>(newValue);
        }

        /// <summary>
        /// Cleanup the subscription to the underlying Observable
        /// </summary>
        /// <param name="disposing">Whether to cleanup managed resources</param>
        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            this.source = null;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Store the updated value into the current value, and set the updated value to null.
        /// </summary>
        private void ApplyValueUpdate()
        {
            this.Value = this.update.Value();
            this.update = Maybe<T>.Null;
        }

        /// <summary>
        /// Listen to the underlying event for publishings
        /// </summary>
        /// <returns>The ISubscription registered with the underlying event.</returns>
        private ISubscription<T> Subscribe()
        {
            return Transaction.Start(this.Subscribe);
        }

        /// <summary>
        /// Listen to the underlying event for publishings
        /// </summary>
        /// <param name="transaction">The transaction to schedule the subscription on.</param>
        /// <returns>The ISubscription registered with the underlying event.</returns>
        private ISubscription<T> Subscribe(Transaction transaction)
        {
            var callback = new SubscriptionPublisher<T>(this.ScheduleApplyValueUpdate);
            var result = this.source.CreateSubscription(callback, Priority.Max, transaction);
            return result;
        }
    }
}