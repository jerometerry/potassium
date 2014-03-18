namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for Event and Behavior
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired through the Observable</typeparam>
    public class Event<T> : Observable<T>, IEvent<T>
    {
        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        /// <typeparam name="TS">The return type of the snapshot function</typeparam>
        /// <param name="initState">The initial state of the behavior</param>
        /// <param name="snapshot">The snapshot generation function</param>
        /// <returns>A new Behavior starting with the given value, that updates 
        /// whenever the current event fires, getting a value computed by the snapshot function.</returns>
        public IBehavior<TS> Accum<TS>(TS initState, Func<T, TS, TS> snapshot)
        {
            return Behavior<T>.Accum(this, initState, snapshot);
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
        public IEvent<T> Coalesce(Func<T, T, T> coalesce)
        {
            return this.StartTransaction(t => new Coalesce<T>(this, coalesce, t));
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="TB">The return type of the new Event</typeparam>
        /// <typeparam name="TS">The snapshot type</typeparam>
        /// <param name="initState">The initial state for the internal Behavior</param>
        /// <param name="snapshot">The mealy machine</param>
        /// <returns>An Event that collects new values</returns>
        public IEvent<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            return new Collect<T, TB, TS>(this, initState, snapshot);
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        /// <returns>An event that is fired with the lowest priority in the current Transaction the current Event is fired in.</returns>
        public IEvent<T> Delay()
        {
            return new Delay<T>(this);
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        /// <param name="predicate">A predicate used to include firings</param>
        /// <returns>A new Event that is fired when the current Event fires and
        /// the predicate evaluates to true.</returns>
        public IEvent<T> Filter(Func<T, bool> predicate)
        {
            return new Filter<T>(this, predicate);
        }

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <returns>A new Event that fires whenever the current Event fires with a non-null value</returns>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        public IEvent<T> FilterNotNull()
        {
            return new NotNullFilter<T>(this);
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        /// <param name="predicate">A behavior who's current value acts as a predicate</param>
        /// <returns>A new Event that fires whenever the current Event fires and the Behaviors value
        /// is true.</returns>
        public IEvent<T> Gate(IValue<bool> predicate)
        {
            return new Gate<T>(this, predicate);
        }

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        /// </summary>
        /// <param name="initValue">The initial value for the Behavior</param>
        /// <returns>A Behavior that updates when the current event is fired,
        /// having the specified initial value.</returns>
        public IBehavior<T> Hold(T initValue)
        {
            return Behavior<T>.Hold(this, initValue);
        }

        /// <summary>
        /// Map firings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that fires whenever the current Event fires, the
        /// the mapped value is computed using the supplied mapping.</returns>
        public IEvent<TB> Map<TB>(Func<T, TB> map)
        {
            return new Map<T, TB>(this, map);
        }

        /// <summary>
        /// Merge two streams of events of the same type.
        /// </summary>
        /// <param name="source">The Event to merge with the current Event</param>
        /// <returns>A new Event that fires whenever either the current or source Events fire.</returns>
        /// <remarks>
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event firings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </remarks>
        public IEvent<T> Merge(IObservable<T> source)
        {
            return new Merge<T>(this, source);
        }

        /// <summary>
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        /// </summary>
        /// <param name="source">The Event to merge with the current Event</param>
        /// <param name="coalesce">The coalesce function that combines simultaneous firings.</param>
        /// <returns>An Event that is fired whenever the current or source Events fire, where
        /// simultaneous firings are handled by the coalesce function.</returns>
        /// <remarks>
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        /// </remarks>
        public IEvent<T> Merge(IObservable<T> source, Func<T, T, T> coalesce)
        {
            var merge = this.Merge(source);
            var c = merge.Coalesce(coalesce);
            c.Register(merge);
            return c;
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        /// <returns>An Event that only fires one time, the first time the current event fires.</returns>
        public IEvent<T> Once()
        {
            return new Once<T>(this);
        }

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <param name="valueStream">The Behavior to sample when calculating the snapshot</param>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <returns>A new Event that will produce the snapshot when the current event fires</returns>
        public IEvent<TC> Snapshot<TB, TC>(IValue<TB> valueStream, Func<T, TB, TC> snapshot)
        {
            return new Snapshot<T, TB, TC>(this, snapshot, valueStream);
        }

        /// <summary>
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <param name="valueStream">The Behavior to sample when taking the snapshot</param>
        /// <returns>An event that captures the Behaviors value when the current event fires</returns>
        public IEvent<TB> Snapshot<TB>(IValue<TB> valueStream)
        {
            return Snapshot(valueStream, (a, b) => b);
        }
    }
}
