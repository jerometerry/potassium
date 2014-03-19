namespace Sodium
{
    using System;

    /// <summary>
    /// An Observer observes an IObservable, forwarding all calls to the IObservable.
    /// </summary>
    /// <typeparam name="T">The type of value fired through the IObservable</typeparam>
    public abstract class Observer<T> : TransactionalObject, IObservable<T>
    {
        private IObservable<T> source;

        /// <summary>
        /// Constructs a new Observer
        /// </summary>
        /// <param name="source">The IObservable to forward all calls to.</param>
        protected Observer(IObservable<T> source)
        {
            this.source = source;
        }

        public ISubscription<T> Subscribe(Action<T> callback)
        {
            return source.Subscribe(callback);
        }

        public ISubscription<T> Subscribe(INotification<T> callback)
        {
            return source.Subscribe(callback);
        }

        public ISubscription<T> Subscribe(INotification<T> callback, Rank subscriptionRank)
        {
            return source.Subscribe(callback, subscriptionRank);
        }

        public ISubscription<T> Subscribe(INotification<T> callback, Rank superior, Transaction transaction)
        {
            return source.Subscribe(callback, superior, transaction);
        }

        public bool CancelSubscription(ISubscription<T> subscription)
        {
            return source.CancelSubscription(subscription);
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
