namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for Event and Behavior
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired through the Observable</typeparam>
    public abstract class Observable<T> : TransactionalObject, IObservable<T>
    {
        /// <summary>
        /// Listen to the Observable for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the event fires</param>
        /// <returns>The event subscription</returns>
        public abstract ISubscription<T> Subscribe(Action<T> callback);

        public abstract ISubscription<T> Subscribe(ISodiumCallback<T> callback);

        public abstract ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank subscriptionRank);

        public abstract ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank superior, Transaction transaction);

        public abstract bool CancelSubscription(ISubscription<T> subscription);
    }
}
