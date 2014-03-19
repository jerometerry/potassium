namespace Sodium
{
    using System;

    /// <summary>
    /// IObservable is the interface for Observable objects (Events and Behaviors) in Sodium.net
    /// </summary>
    /// <typeparam name="T">The type of value published through the IObservable</typeparam>
    public interface IObservable<T> : IDisposable
    {
        /// <summary>
        /// Listen for publishings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event publishes.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        ISubscription<T> Subscribe(Action<T> callback);

        /// <summary>
        /// Listen for publishings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event publishes.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        ISubscription<T> Subscribe(INotification<T> callback);

        /// <summary>
        /// Listen for publishings on the current event
        /// </summary>
        /// <param name="callback">The action to invoke on a publishing</param>
        /// <param name="subscriptionRank">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An ISubscription to be used to stop listening for events</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// Subscribe operation that takes a thread. This ensures that any other
        /// actions triggered during Subscribe requiring a transaction all get the same instance.</remarks>
        ISubscription<T> Subscribe(INotification<T> callback, Rank subscriptionRank);

        /// <summary>
        /// Listen for publishings on the current event
        /// </summary>
        /// <param name="transaction">Transaction to send any publishings on</param>
        /// <param name="callback">The action to invoke on a publishing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An ISubscription to be used to stop listening for events.</returns>
        /// <remarks>Any publishings that have occurred on the current transaction will be re-published immediate after subscribing.</remarks>
        ISubscription<T> Subscribe(INotification<T> callback, Rank superior, Transaction transaction);

        /// <summary>
        /// Stop the given subscription from receiving updates from the current Event
        /// </summary>
        /// <param name="subscription">The subscription to remove</param>
        /// <returns>True if the subscription was removed, false otherwise</returns>
        bool CancelSubscription(ISubscription<T> subscription);
    }
}