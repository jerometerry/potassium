namespace Sodium
{
    using System;

    /// <summary>
    /// Reactive extensions for Observables
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <typeparam name="TS">The return type of the snapshot function</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="value">The initial state of the behavior</param>
        /// <param name="snapshot">The snapshot generation function</param>
        /// <returns>A new Behavior starting with the given value, that updates 
        /// whenever the current event publishes, getting a value computed by the snapshot function.</returns>
        public static Behavior<TS> Accum<T, TS>(this Observable<T> source, TS value, Func<T, TS, TS> snapshot)
        {
            var evt = new EventLoop<TS>();
            var behavior = evt.Hold(value);

            var snapshotEvent = Snapshot(source, behavior, snapshot);
            evt.Loop(snapshotEvent);

            var result = snapshotEvent.Hold(value);
            result.Register(evt);
            result.Register(behavior);
            result.Register(snapshotEvent);

            return result;
        }

        /// <summary>
        /// If there's more than one publishing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="coalesce">A function that takes two publishings of the same type, and returns
        /// produces a new publishing of the same type.</param>
        /// <returns>A new Event that publishes the coalesced values</returns>
        /// <remarks>
        /// If the event publishings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        public static Event<T> Coalesce<T>(this Observable<T> source, Func<T, T, T> coalesce)
        {
            return Transaction.Start(t => new CoalesceEvent<T>(source, coalesce, t));
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">The return type of the new Event</typeparam>
        /// <typeparam name="TS">The snapshot type</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="initState">The initial state for the internal Behavior</param>
        /// <param name="snapshot">The mealy machine</param>
        /// <returns>An Event that collects new values</returns>
        public static Event<TB> Collect<TA, TB, TS>(this Observable<TA> source, TS initState, Func<TA, TS, Tuple<TB, TS>> snapshot)
        {
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
            var ebs = source.Snapshot(s, snapshot);
            var eb = ebs.Map(bs => bs.Item1);
            var evt = ebs.Map(bs => bs.Item2);
            es.Loop(evt);

            eb.Register(es);
            eb.Register(s);
            eb.Register(ebs);
            eb.Register(evt);

            return eb;
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Event</param>
        /// <returns>An event that is published with the lowest priority in the current Transaction the current Event is published in.</returns>
        public static Event<T> Delay<T>(this Observable<T> source)
        {
            var evt = new EventPublisher<T>();
            var callback = new Publisher<T>((a, t) => t.Low(() => evt.Publish(a)));
            var subscription = source.Subscribe(callback, evt.Rank);
            evt.Register(subscription);
            return evt;
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="predicate">A predicate used to include publishings</param>
        /// <returns>A new Event that is published when the current Event publishes and
        /// the predicate evaluates to true.</returns>
        public static Event<T> Filter<T>(this Observable<T> source, Func<T, bool> predicate)
        {
            return new FilterEvent<T>(source, predicate);
        }

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Event</param>
        /// <returns>A new Event that publishes whenever the current Event publishes with a non-null value</returns>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        public static Event<T> FilterNotNull<T>(this Observable<T> source)
        {
            return new FilterEvent<T>(source, a => a != null);
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="predicate">A behavior who's current value acts as a predicate</param>
        /// <returns>A new Event that publishes whenever the current Event publishes and the Behaviors value
        /// is true.</returns>
        public static Event<T> Gate<T>(this Observable<T> source, Behavior<bool> predicate)
        {
            Func<T, bool, Maybe<T>> snapshot = (a, p) => p ? new Maybe<T>(a) : null;
            var sn = source.Snapshot(predicate, snapshot);
            var filter = sn.FilterNotNull();
            var map = filter.Map(a => a.Value());
            map.Register(filter);
            map.Register(sn);
            return map;
        }

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event publishings get processed at the end of
        /// the transaction.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="value">The initial value for the Behavior</param>
        /// <returns>A Behavior that updates when the current event is published,
        /// having the specified initial value.</returns>
        public static Behavior<T> Hold<T>(this Observable<T> source, T value)
        {
            return Transaction.Start(t => Hold(source, value, t));
        }

        /// <summary>
        /// Creates a Behavior from an Observable and an initial value
        /// </summary>
        /// <param name="source">The source to update the Behavior from</param>
        /// <param name="value">The initial value of the Behavior</param>
        /// <param name="t">The Transaction to perform the Hold</param>
        /// <returns>The Behavior with the given value</returns>
        public static Behavior<T> Hold<T>(this Observable<T> source, T value, Transaction t)
        {
            var s = new LastFiringEvent<T>(source, t);
            var b = new Behavior<T>(s, value);
            b.Register(s);
            return b;
        }

        /// <summary>
        /// Map publishings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that publishes whenever the current Event publishes, the
        /// the mapped value is computed using the supplied mapping.</returns>
        public static Event<TB> Map<TA, TB>(this Observable<TA> source, Func<TA, TB> map)
        {
            return new MapEvent<TA, TB>(source, map);
        }

        /// <summary>
        /// Merge two streams of events of the same type.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="observable">The Event to merge with the current Event</param>
        /// <returns>A new Event that publishes whenever either the current or source Events publish.</returns>
        /// <remarks>
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event publishings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </remarks>
        public static Event<T> Merge<T>(this Observable<T> source, Observable<T> observable)
        {
            return new MergeEvent<T>(source, observable);
        }

        /// <summary>
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="observable">The Event to merge with the current Event</param>
        /// <param name="coalesce">The coalesce function that combines simultaneous publishings.</param>
        /// <returns>An Event that is published whenever the current or source Events publish, where
        /// simultaneous publishings are handled by the coalesce function.</returns>
        /// <remarks>
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        /// </remarks>
        public static Event<T> Merge<T>(this Observable<T> source, Observable<T> observable, Func<T, T, T> coalesce)
        {
            var merge = source.Merge(observable);
            var c = merge.Coalesce(coalesce);
            c.Register(merge);
            return c;
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Event</param>
        /// <returns>An Event that only publishes one time, the first time the current event publishes.</returns>
        public static Event<T> Once<T>(this Observable<T> source)
        {
            return new OnceEvent<T>(source);
        }

        /// <summary>
        /// Sample the behavior at the time of the event publishing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="valueStream">The Behavior to sample when calculating the snapshot</param>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <returns>A new Event that will produce the snapshot when the current event publishes</returns>
        public static Event<TC> Snapshot<TA, TB, TC>(this Observable<TA> source, Behavior<TB> valueStream, Func<TA, TB, TC> snapshot)
        {
            return new SnapshotEvent<TA, TB, TC>(source, snapshot, valueStream);
        }

        /// <summary>
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="valueStream">The Behavior to sample when taking the snapshot</param>
        /// <returns>An event that captures the Behaviors value when the current event publishes</returns>
        public static Event<TB> Snapshot<TA, TB>(this Observable<TA> source, Behavior<TB> valueStream)
        {
            return source.Snapshot(valueStream, (a, b) => b);
        }
    }
}
