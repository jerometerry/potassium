using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sodium
{
    /// <summary>
    /// Interface required for sources for use with Behaviors
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Behavior.</typeparam>
    public interface IBehaviorSource<T> : IDisposable
    {
        /// <summary>
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <param name="coalesce">A function that takes two firings of the same type, and returns
        /// produces a new firing of the same type.</param>
        /// <returns>A new Event that fires the coalesced values</returns>
        /// <remarks>
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        Event<T> Coalesce(Func<T, T, T> coalesce);

        /// <summary>
        /// Map firings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that fires whenever the current Event fires, the
        /// the mapped value is computed using the supplied mapping.</returns>
        Event<TB> Map<TB>(Func<T, TB> map);

        /// <summary>
        /// Listen for firings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event fires.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        ISubscription<T> Subscribe(Action<T> callback);

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="rank">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An ISubscription to be used to stop listening for events</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// Subscribe operation that takes a thread. This ensures that any other
        /// actions triggered during Subscribe requiring a transaction all get the same instance.</remarks>
        ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank rank);

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="transaction">Transaction to send any firings on</param>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="rank">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An ISubscription to be used to stop listening for events.</returns>
        /// <remarks>Any firings that have occurred on the current transaction will be re-fired immediate after subscribing.</remarks>
        ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank rank, Transaction transaction);
    }
}
