namespace Sodium
{
    using System;

    /// <summary>
    /// An Observer observes an Observable, forwarding all calls to the Observable.
    /// </summary>
    /// <typeparam name="T">The type of value published through the Observable</typeparam>
    public abstract class Observer<T> : Observable<T>
    {
        private Observable<T> source;

        /// <summary>
        /// Constructs a new Observer
        /// </summary>
        /// <param name="source">The Observable to forward all calls to.</param>
        protected Observer(Observable<T> source)
        {
            this.source = source;
        }

        public override bool CancelSubscription(ISubscription<T> subscription)
        {
            return source.CancelSubscription(subscription);
        }

        public override ISubscription<T> Subscribe(Action<T> callback)
        {
            return source.Subscribe(callback);
        }

        internal override ISubscription<T> Subscribe(IPublisher<T> callback)
        {
            return source.Subscribe(callback);
        }

        internal override ISubscription<T> Subscribe(IPublisher<T> callback, Rank subscriptionRank)
        {
            return source.Subscribe(callback, subscriptionRank);
        }

        internal override ISubscription<T> Subscribe(IPublisher<T> callback, Rank superior, Transaction transaction)
        {
            return source.Subscribe(callback, superior, transaction);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.source = null;
            }

            base.Dispose(disposing);
        }
    }
}
