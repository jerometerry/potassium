namespace Potassium.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Potassium.Internal;
    using Potassium.Utilities;

    /// <summary>
    /// Observable is the base class for Events, containing the subscription logic (i.e. the Observer Pattern).
    /// </summary>
    /// <typeparam name="T">The type of value fired through the Observable</typeparam>
    public abstract class Observable<T> : Disposable, IObservable<T>
    {
        /// <summary>
        /// The rank of the current Observable. Default to rank zero
        /// </summary>
        private readonly Priority priority = new Priority();

        /// <summary>
        /// List of ISubscriptions that are currently listening for firings
        /// from the current Observable.
        /// </summary>
        private readonly List<ISubscription<T>> subscriptions = new List<ISubscription<T>>();

        /// <summary>
        /// The current Rank of the Observable, used to prioritize firings on the current transaction.
        /// </summary>
        public Priority Priority
        {
            get
            {
                return this.priority;
            }
        }

        /// <summary>
        /// List of ISubscriptions that are currently listening for firings
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
        /// <param name="callback">An Action to be invoked when the current Observable fires values.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        public ISubscription<T> Subscribe(Action<T> callback)
        {
            return Transaction.Start(t => this.Subscribe(new Observer<T>(callback), Priority.Max, t));
        }

        /// <summary>
        /// Subscribe to publications of the current Observable.
        /// </summary>
        /// <param name="observer">The action to invoke on a firing</param>
        /// <param name="subscriptionRank">A rank that will be added as a superior of the Rank of the current Observable</param>
        /// <returns>An ISubscription to be used to stop listening for Observables</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// Subscribe operation that takes a thread. This ensures that any other
        /// actions triggered during Subscribe requiring a transaction all get the same instance.</remarks>
        internal ISubscription<T> Subscribe(Observer<T> observer, Priority subscriptionRank)
        {
            return Transaction.Start(t => this.Subscribe(observer, subscriptionRank, t));
        }

        /// <summary>
        /// Subscribe to publications of the current Observable.
        /// </summary>
        /// <param name="transaction">Transaction to send any firings on</param>
        /// <param name="observer">The action to invoke on a firing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Observable</param>
        /// <returns>An ISubscription to be used to stop listening for Observables.</returns>
        /// <remarks>Any firings that have occurred on the current transaction will be re-fired immediate after subscribing.</remarks>
        internal ISubscription<T> Subscribe(Observer<T> observer, Priority superior, Transaction transaction)
        {
            Subscription<T> subscription;

            if (Monitor.TryEnter(Constants.SubscriptionLock, Constants.LockTimeout))
            {
                try
                { 
                    if (this.priority.AddSuperior(superior))
                    {
                        transaction.Reprioritize = true;
                    }

                    subscription = new Subscription<T>(this, observer, superior);
                    this.Subscriptions.Add(subscription);
                }
                finally
                {
                    Monitor.Exit(Constants.SubscriptionLock);
                }
            }
            else
            {
                throw new InvalidOperationException("Could not obtain the subscription lock while creating a subscription");
            }

            this.OnSubscribe(subscription, transaction);
            return subscription;
        }

        /// <summary>
        /// Stop the given subscription from receiving updates from the current Observable
        /// </summary>
        /// <param name="subscription">The subscription to remove</param>
        /// <returns>True if the subscription was removed, false otherwise</returns>
        internal bool CancelSubscription(ISubscription<T> subscription)
        {
            if (subscription == null)
            {
                return false;
            }

            var s = (Subscription<T>)subscription;

            if (Monitor.TryEnter(Constants.SubscriptionLock, Constants.LockTimeout))
            {
                try
                { 
                    Priority.RemoveSuperior(s.Priority);
                    return this.Subscriptions.Remove(s);
                }
                finally
                {
                    Monitor.Exit(Constants.SubscriptionLock);
                }
            }
            
            throw new InvalidOperationException("Could not obtain the subscription lock while canceling a subscription");
        }

        /// <summary>
        /// Performs additional steps during the subscription process
        /// </summary>
        /// <param name="subscription">The newly created subscription</param>
        /// <param name="transaction">The current transaction</param>
        internal virtual void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
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
