namespace Potassium.Core
{
    using System;
    using Potassium.Internal;
    using Potassium.Providers;

    /// <summary>
    /// An Event is a discrete stream of occurrences (e.g. Mouse Clicks, Key Presses, etc.).
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired through the Event</typeparam>
    public class Event<T> : Observable<T>
    {
        /// <summary>
        /// Overload the And operator to filter Events
        /// </summary>
        /// <param name="e">The Event to filter</param>
        /// <param name="p">The filter predicate</param>
        /// <returns>The filtered Event</returns>
        public static Event<T> operator &(Event<T> e, Func<T, bool> p)
        {
            return e.Filter(p);
        }

        /// <summary>
        /// Overload the Or operator to merge two Events together
        /// </summary>
        /// <param name="e">The Event</param>
        /// <param name="o">The Observable to merge with the Event</param>
        /// <returns>The merged Event</returns>
        public static Event<T> operator |(Event<T> e, Observable<T> o)
        {
            return e.Merge(o);
        }

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
        public Event<T> Coalesce(Func<T, T, T> coalesce)
        {
            return Transaction.Start(t => new CoalesceEvent<T>(this, coalesce, t));
        }

        /// <summary>
        /// Create a new Event that fires whenever the current Event fires.
        /// </summary>
        /// <returns>A new Event that fires whenever the source Event fires.</returns>
        public Event<T> Clone()
        {
            var feed = new EventFeed<T>();
            feed.Feed(this);
            return feed;
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        /// <returns>An event that is fired with the lowest priority in the current Transaction the current Event is fired in.</returns>
        public Event<T> Delay()
        {
            return new DelayEvent<T>(this);
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        /// <param name="predicate">A predicate used to include firings</param>
        /// <returns>A new Event that is fired when the current Event fires and
        /// the predicate evaluates to true.</returns>
        public Event<T> Filter(Func<T, bool> predicate)
        {
            return new FilteredEvent<T>(this, predicate);
        }

        /// <summary>
        /// Let event occurrences through only when the predicate is True.
        /// </summary>
        /// <param name="predicate">The predicate function</param>
        /// <returns>A new Event that fires whenever the current Event fires and the predicate
        /// is true.</returns>
        public Event<T> Gate(Func<bool> predicate)
        {
            return new GateEvent<T>(this, predicate);
        }

        /// <summary>
        /// Let event occurrences through only when the predicate True.
        /// </summary>
        /// <param name="predicate">The predicate</param>
        /// <returns>A new Event that fires whenever the current Event fires and the predicate
        /// is true.</returns>
        public Event<T> Gate(IProvider<bool> predicate)
        {
            return new GateEvent<T>(this, predicate);
        }

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        /// </summary>
        /// <param name="value">The initial value for the Behavior</param>
        /// <returns>A Behavior that updates when the current event is fired,
        /// having the specified initial value.</returns>
        public Behavior<T> Hold(T value)
        {
            return Transaction.Start(t => new HoldBehavior<T>(this, value, t));
        }

        /// <summary>
        /// Gets an Event that only fires once if the current Event fires multiple times in the same transaction
        /// </summary>
        /// <returns>The Event that fires only the last event in a Transaction</returns>
        public Event<T> LastFiring()
        {
            return Transaction.Start(t => new LastFiringEvent<T>(this, t));
        }

        /// <summary>
        /// Map firings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that fires whenever the current Event fires, the
        /// the mapped value is computed using the supplied mapping.</returns>
        public Event<TB> Map<TB>(Func<T, TB> map)
        {
            return new MapEvent<T, TB>(this, map);
        }

        /// <summary>
        /// Merge two streams of events of the same type.
        /// </summary>
        /// <param name="observable">The Event to merge with the current Event</param>
        /// <returns>A new Event that fires whenever either the current or source Events fire.</returns>
        /// <remarks>
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event firings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </remarks>
        public Event<T> Merge(Observable<T> observable)
        {
            return new MergeEvent<T>(this, observable);
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        /// <returns>An Event that only fires one time, the first time the current event fires.</returns>
        public Event<T> Once()
        {
            return new OnceEvent<T>(this);
        }

        /// <summary>
        /// Sample the behavior at the time of the event firing. 
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <param name="provider">The Behavior to sample when calculating the snapshot</param>
        /// <returns>A new Event that will produce the snapshot when the current event fires</returns>
        /// <remarks>Note that the 'current value' of the behavior that's sampled is the value 
        /// as at the start of the transaction before any state changes of the current transaction 
        /// are applied through 'hold's.</remarks>
        public Event<TC> Snapshot<TB, TC>(Func<T, TB, TC> snapshot, IProvider<TB> provider)
        {
            return new SnapshotEvent<T, TB, TC>(this, snapshot, provider);
        }

        /// <summary>
        /// Sample the providers value at the time of the event firing
        /// </summary>
        /// <typeparam name="TB">The type of the Provider</typeparam>
        /// <param name="provider">The IProvider to sample when taking the snapshot</param>
        /// <returns>An event that captures the IProviders value when the current event fires</returns>
        public Event<TB> Snapshot<TB>(IProvider<TB> provider)
        {
            return this.Snapshot((a, b) => b, provider);
        }
    }
}
