namespace Sodium.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Observable is the base class for Observables and Behaviors, containing the subscription logic (i.e. the Observer Pattern).
    /// </summary>
    /// <typeparam name="T">The type of value published through the Observable</typeparam>
    public abstract class Observable<T> : DisposableObject
    {
        /// <summary>
        /// The rank of the current Observable. Default to rank zero
        /// </summary>
        private readonly Priority priority = new Priority();

        /// <summary>
        /// List of ISubscriptions that are currently listening for publishings 
        /// from the current Observable.
        /// </summary>
        private readonly List<ISubscription<T>> subscriptions = new List<ISubscription<T>>();

        /// <summary>
        /// The current Rank of the Observable, used to prioritize publishings on the current transaction.
        /// </summary>
        public Priority Priority
        {
            get
            {
                return this.priority;
            }
        }

        /// <summary>
        /// List of ISubscriptions that are currently listening for publishings 
        /// from the current Observable.
        /// </summary>
        protected List<ISubscription<T>> Subscriptions
        {
            get
            {
                return this.subscriptions;
            }
        }

        /// <summary>
        /// Subscribe to publications of the current Observable.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Observable publishes values.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        public ISubscription<T> Subscribe(Action<T> callback)
        {
            return Transaction.Start(t => this.CreateSubscription(new SubscriptionPublisher<T>(callback), Priority.Max, t));
        }

        /// <summary>
        /// Subscribe to publications of the current Observable.
        /// </summary>
        /// <param name="transaction">Transaction to send any publishings on</param>
        /// <param name="publisher">The action to invoke on a publishing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Observable</param>
        /// <returns>An ISubscription to be used to stop listening for Observables.</returns>
        /// <remarks>Any publishings that have occurred on the current transaction will be re-published immediate after subscribing.</remarks>
        internal virtual ISubscription<T> CreateSubscription(SubscriptionPublisher<T> publisher, Priority superior, Transaction transaction)
        {
            Subscription<T> subscription;
            lock (Constants.SubscriptionLock)
            {
                if (this.priority.AddSuperior(superior))
                {
                    transaction.Reprioritize = true;
                }

                subscription = new Subscription<T>(this, publisher, superior);
                this.Subscriptions.Add(subscription);
            }

            this.OnSubscribe(subscription, transaction);
            return subscription;
        }

        /// <summary>
        /// Subscribe to publications of the current Observable.
        /// </summary>
        /// <param name="publisher">An Action to be invoked when the current Observable publishes new values.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        internal ISubscription<T> Subscribe(SubscriptionPublisher<T> publisher)
        {
            return Transaction.Start(t => this.CreateSubscription(publisher, Priority.Max, t));
        }

        /// <summary>
        /// Subscribe to publications of the current Observable.
        /// </summary>
        /// <param name="publisher">The action to invoke on a publishing</param>
        /// <param name="subscriptionRank">A rank that will be added as a superior of the Rank of the current Observable</param>
        /// <returns>An ISubscription to be used to stop listening for Observables</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// Subscribe operation that takes a thread. This ensures that any other
        /// actions triggered during Subscribe requiring a transaction all get the same instance.</remarks>
        internal ISubscription<T> Subscribe(SubscriptionPublisher<T> publisher, Priority subscriptionRank)
        {
            return Transaction.Start(t => this.CreateSubscription(publisher, subscriptionRank, t));
        }

        /// <summary>
        /// Stop the given subscription from receiving updates from the current Observable
        /// </summary>
        /// <param name="subscription">The subscription to remove</param>
        /// <returns>True if the subscription was removed, false otherwise</returns>
        internal virtual bool CancelSubscription(ISubscription<T> subscription)
        {
            if (subscription == null)
            {
                return false;
            }

            var s = (Subscription<T>)subscription;

            lock (Constants.SubscriptionLock)
            {
                Priority.RemoveSuperior(s.Priority);
                return this.Subscriptions.Remove(s);
            }
        }

        /// <summary>
        /// Performs additional steps during the subscription process
        /// </summary>
        /// <param name="subscription">The newly created subscription</param>
        /// <param name="transaction">The current transaction</param>
        protected virtual void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
        {
        }

        /// <summary>
        /// Cleanup the current Observable, disposing of any subscriptions.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            var clone = this.Subscriptions.ToArray();
            this.Subscriptions.Clear();
            foreach (var subscription in clone)
            {
                subscription.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
