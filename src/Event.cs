namespace JT.Rx.Net
{
    using System;
    using JT.Rx.Net.Internal;

    /// <summary>
    /// An Event is a discrete signal of values.
    /// </summary>
    /// <typeparam name="T">The type of value that will be published through the Event</typeparam>
    public class Event<T> : Observable<T>
    {
        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        /// <typeparam name="TS">The return type of the snapshot function</typeparam>
        /// <param name="value">The initial state of the behavior</param>
        /// <param name="accumulator">The snapshot generation function</param>
        /// <returns>A new Behavior starting with the given value, that updates 
        /// whenever the current event publishes, getting a value computed by the snapshot function.</returns>
        public Behavior<TS> Accum<TS>(TS value, Func<T, TS, TS> accumulator)
        {
            var eventFeed = new EventFeed<TS>();

            // Behavior holds the running snapshot value
            var previousShapshotBehavior = eventFeed.Hold(value);

            // Event that fires the new accumulated values, using the accumulator and the previous values
            var accumulationEvent = this.Snapshot(previousShapshotBehavior, accumulator);

            // Feed the new accumulated values into the Behavior, to store the new snapshot as the previous snapshot
            eventFeed.Feed(accumulationEvent);

            // Behavior that holds the value of the new accumulated values
            var result = accumulationEvent.Hold(value);

            result.Register(eventFeed);
            result.Register(previousShapshotBehavior);
            result.Register(accumulationEvent);

            return result;
        }

        /// <summary>
        /// If there's more than one publishing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <param name="coalesce">A function that takes two publishings of the same type, and returns
        /// produces a new publishing of the same type.</param>
        /// <returns>A new Event that publishes the coalesced values</returns>
        /// <remarks>
        /// If the event publishings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        public Event<T> Coalesce(Func<T, T, T> coalesce)
        {
            return Transaction.Start(t => new CoalesceEvent<T>(this, coalesce, t));
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="TB">The return type of the new Event</typeparam>
        /// <typeparam name="TS">The snapshot type</typeparam>
        /// <param name="initState">The initial state for the internal Behavior</param>
        /// <param name="collector">The mealy machine</param>
        /// <returns>An Event that collects new values</returns>
        public Event<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> collector)
        {
            // snapshotFeed is used to create the Behavior that holds the snapshot values
            var snapshotFeed = new EventFeed<TS>();

            // Behavior that holds the previous collected value
            var snapshotBehavior = snapshotFeed.Hold(initState);

            // Event that emits a Tuple<TB,TS> containing the mapped value and the snapshot
            var mappedEventSnapshot = this.Snapshot(snapshotBehavior, collector);

            // Event that emits the snapshot values from the mappedEventSnapshot above
            var snapshotEvent = mappedEventSnapshot.Map(bs => bs.Item2);

            // Feed the snapshots into the Behavior holding the snapshot values
            snapshotFeed.Feed(snapshotEvent);

            // Event that extracts the mapped value from the mappedEventSnapshot above
            var mappedEvent = mappedEventSnapshot.Map(bs => bs.Item1);

            mappedEvent.Register(snapshotFeed);
            mappedEvent.Register(snapshotBehavior);
            mappedEvent.Register(mappedEventSnapshot);
            mappedEvent.Register(snapshotEvent);

            return mappedEvent;
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <returns>An event that is published with the lowest priority in the current Transaction the current Event is published in.</returns>
        public Event<T> Delay()
        {
            var evt = new EventPublisher<T>();
            var callback = new SubscriptionPublisher<T>((a, t) => t.Low(() => evt.Publish(a)));
            var subscription = this.Subscribe(callback, evt.Priority);
            evt.Register(subscription);
            return evt;
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        /// <param name="predicate">A predicate used to include publishings</param>
        /// <returns>A new Event that is published when the current Event publishes and
        /// the predicate evaluates to true.</returns>
        public Event<T> Filter(Func<T, bool> predicate)
        {
            return new FilterEvent<T>(this, predicate);
        }

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <returns>A new Event that publishes whenever the current Event publishes with a non-null value</returns>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        public Event<T> FilterNotNull()
        {
            return new FilterEvent<T>(this, a => a != null);
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        /// <param name="predicate">A behavior who's current value acts as a predicate</param>
        /// <returns>A new Event that publishes whenever the current Event publishes and the Behaviors value
        /// is true.</returns>
        public Event<T> Gate(Predicate predicate)
        {
            Func<T, bool, Maybe<T>> snapshot = (a, p) => p ? new Maybe<T>(a) : null;
            var sn = this.Snapshot(predicate, snapshot);
            var filter = sn.FilterNotNull();
            var map = filter.Map(a => a.Value);
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
        /// <param name="value">The initial value for the Behavior</param>
        /// <returns>A Behavior that updates when the current event is published,
        /// having the specified initial value.</returns>
        public Behavior<T> Hold(T value)
        {
            return Transaction.Start(t => Hold(value, t));
        }

        /// <summary>
        /// Creates a Behavior from an Observable and an initial value
        /// </summary>
        /// <param name="value">The initial value of the Behavior</param>
        /// <param name="t">The Transaction to perform the Hold</param>
        /// <returns>The Behavior with the given value</returns>
        public Behavior<T> Hold(T value, Transaction t)
        {
            var s = new LastFiringEvent<T>(this, t);
            var b = new Behavior<T>(value, s);
            b.Register(s);
            return b;
        }

        /// <summary>
        /// Map publishings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that publishes whenever the current Event publishes, the
        /// the mapped value is computed using the supplied mapping.</returns>
        public Event<TB> Map<TB>(Func<T, TB> map)
        {
            return new MapEvent<T, TB>(this, map);
        }

        /// <summary>
        /// Merge two streams of events of the same type.
        /// </summary>
        /// <param name="observable">The Event to merge with the current Event</param>
        /// <returns>A new Event that publishes whenever either the current or source Events publish.</returns>
        /// <remarks>
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event publishings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </remarks>
        public Event<T> Merge(Observable<T> observable)
        {
            return new MergeEvent<T>(this, observable);
        }

        /// <summary>
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        /// </summary>
        /// <param name="observable">The Event to merge with the current Event</param>
        /// <param name="coalesce">The coalesce function that combines simultaneous publishings.</param>
        /// <returns>An Event that is published whenever the current or source Events publish, where
        /// simultaneous publishings are handled by the coalesce function.</returns>
        /// <remarks>
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        /// </remarks>
        public Event<T> Merge(Observable<T> observable, Func<T, T, T> coalesce)
        {
            var merge = this.Merge(observable);
            var c = merge.Coalesce(coalesce);
            c.Register(merge);
            return c;
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        /// <returns>An Event that only publishes one time, the first time the current event publishes.</returns>
        public Event<T> Once()
        {
            return new OnceEvent<T>(this);
        }

        /// <summary>
        /// Sample the behavior at the time of the event publishing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <param name="valueSourceStream">The Behavior to sample when calculating the snapshot</param>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <returns>A new Event that will produce the snapshot when the current event publishes</returns>
        public Event<TC> Snapshot<TB, TC>(IValueSource<TB> valueSourceStream, Func<T, TB, TC> snapshot)
        {
            return new SnapshotEvent<T, TB, TC>(this, snapshot, valueSourceStream);
        }

        /// <summary>
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <param name="valueSourceStream">The Behavior to sample when taking the snapshot</param>
        /// <returns>An event that captures the Behaviors value when the current event publishes</returns>
        public Event<TB> Snapshot<TB>(IValueSource<TB> valueSourceStream)
        {
            return this.Snapshot(valueSourceStream, (a, b) => b);
        }
    }
}
