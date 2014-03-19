namespace Sodium
{
    using System;

    /// <summary>
    /// Transformer applies Transforms to Events and Behaviors to produce new Events and Behaviors
    /// </summary>
    public class Transformer : TransactionalObject
    {
        private static Transformer instance;

        /// <summary>
        /// Gets the default instance of the EventTransformer
        /// </summary>
        public static Transformer Default
        {
            get
            {
                return instance ?? (instance = new Transformer());
            }
        }

        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <typeparam name="TS">The return type of the snapshot function</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="initState">The initial state of the behavior</param>
        /// <param name="snapshot">The snapshot generation function</param>
        /// <returns>A new Behavior starting with the given value, that updates 
        /// whenever the current event fires, getting a value computed by the snapshot function.</returns>
        public IBehavior<TS> Accum<T, TS>(IEvent<T> source, TS initState, Func<T, TS, TS> snapshot)
        {
            var evt = new EventLoop<TS>();
            var behavior = evt.Hold(initState);

            var snapshotEvent = source.Snapshot(behavior, snapshot);
            evt.Loop(snapshotEvent);

            var result = snapshotEvent.Hold(initState);
            result.Register(evt);
            result.Register(behavior);
            result.Register(snapshotEvent);

            return result;
        }

        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        /// <typeparam name="TB">The return type of the inner function of the given Behavior</typeparam>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source"> </param>
        /// <param name="bf">Behavior of functions that maps from T -> TB</param>
        /// <returns>The new applied Behavior</returns>
        public IBehavior<TB> Apply<T, TB>(IBehavior<T> source, IBehavior<Func<T, TB>> bf)
        {
            var evt = new BehaviorApplyEvent<T, TB>(source, bf);
            var map = bf.Value;
            var valA = source.Value;
            var valB = map(valA);
            var behavior = evt.Hold(valB);
            behavior.Register(evt);
            return behavior;
        }

        /// <summary>
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="coalesce">A function that takes two firings of the same type, and returns
        /// produces a new firing of the same type.</param>
        /// <returns>A new Event that fires the coalesced values</returns>
        /// <remarks>
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        public IEvent<T> Coalesce<T>(IEvent<T> source, Func<T, T, T> coalesce)
        {
            return this.StartTransaction(t => new CoalesceEvent<T>(source, coalesce, t));
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <typeparam name="TB">The return type of the new Event</typeparam>
        /// <typeparam name="TS">The snapshot type</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="initState">The initial state for the internal Behavior</param>
        /// <param name="snapshot">The mealy machine</param>
        /// <returns>An Event that collects new values</returns>
        public IEvent<TB> Collect<T, TB, TS>(IEvent<T> source, TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            return new CollectEvent<T, TB, TS>(source, initState, snapshot);
        }

        /// <summary>
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="TB">The type of the returned Behavior</typeparam>
        /// <typeparam name="TS">The snapshot function</typeparam>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source"> </param>
        /// <param name="initState">Value to pass to the snapshot function</param>
        /// <param name="snapshot">Snapshot function</param>
        /// <returns>A new Behavior that collects values of type TB</returns>
        public IBehavior<TB> Collect<T, TB, TS>(IBehavior<T> source, TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var coalesceEvent = source.Source.Coalesce((a, b) => b);
            var currentValue = source.Value;
            var tuple = snapshot(currentValue, initState);
            var loop = new EventLoop<Tuple<TB, TS>>();
            var loopBehavior = loop.Hold(tuple);
            var snapshotBehavior = loopBehavior.Map(x => x.Item2);
            var coalesceSnapshotEvent = coalesceEvent.Snapshot(snapshotBehavior, snapshot);
            loop.Loop(coalesceSnapshotEvent);

            var result = loopBehavior.Map(x => x.Item1);
            result.Register(loop);
            result.Register(loopBehavior);
            result.Register(coalesceEvent);
            result.Register(coalesceSnapshotEvent);
            result.Register(snapshotBehavior);
            return result;
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <returns>An event that is fired with the lowest priority in the current Transaction the current Event is fired in.</returns>
        public IEvent<T> Delay<T>(IEvent<T> source)
        {
            return new DelayEvent<T>(source);
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="predicate">A predicate used to include firings</param>
        /// <returns>A new Event that is fired when the current Event fires and
        /// the predicate evaluates to true.</returns>
        public IEvent<T> Filter<T>(IEvent<T> source, Func<T, bool> predicate)
        {
            return new FilterEvent<T>(source, predicate);
        }

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <returns>A new Event that fires whenever the current Event fires with a non-null value</returns>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        public IEvent<T> FilterNotNull<T>(IEvent<T> source)
        {
            return new NotNullFilterEvent<T>(source);
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="predicate">A behavior who's current value acts as a predicate</param>
        /// <returns>A new Event that fires whenever the current Event fires and the Behaviors value
        /// is true.</returns>
        public IEvent<T> Gate<T>(IEvent<T> source, IValue<bool> predicate)
        {
            return new GateEvent<T>(source, predicate);
        }

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="initValue">The initial value for the Behavior</param>
        /// <returns>A Behavior that updates when the current event is fired,
        /// having the specified initial value.</returns>
        public IBehavior<T> Hold<T>(IObservable<T> source, T initValue)
        {
            return this.StartTransaction(t => Hold(source, initValue, t));
        }

        /// <summary>
        /// Creates a Behavior from an Observable and an initial value
        /// </summary>
        /// <param name="source">The source to update the Behavior from</param>
        /// <param name="initValue">The initial value of the Behavior</param>
        /// <param name="t">The Transaction to perform the Hold</param>
        /// <returns>The Behavior with the given value</returns>
        public IBehavior<T> Hold<T>(IObservable<T> source, T initValue, Transaction t)
        {
            var s = new LastFiringEvent<T>(source, t);
            var b = new Behavior<T>(s, initValue);
            b.Register(s);
            return b;
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        /// <typeparam name="TB">The type of the given Behavior</typeparam>
        /// <typeparam name="TC">The return type of the lift function.</typeparam>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source"> </param>
        /// <param name="lift">The function to lift, taking a T and a TB, returning TC</param>
        /// <param name="behavior">The behavior used to apply a partial function by mapping the given 
        /// lift method to the current Behavior.</param>
        /// <returns>A new Behavior who's value is computed using the current Behavior, the given
        /// Behavior, and the lift function.</returns>
        public IBehavior<TC> Lift<T, TB, TC>(IBehavior<T> source, Func<T, TB, TC> lift, IBehavior<TB> behavior)
        {
            Func<T, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            var bf = source.Map(ffa);
            var result = behavior.Apply(bf);
            result.Register(bf);
            return result;
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        /// <typeparam name="TB">Type of Behavior b</typeparam>
        /// <typeparam name="TC">Type of Behavior c</typeparam>
        /// <typeparam name="TD">Return type of the lift function</typeparam>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source"> </param>
        /// <param name="lift">The function to lift</param>
        /// <param name="b">Behavior of type TB used to do the lift</param>
        /// <param name="c">Behavior of type TC used to do the lift</param>
        /// <returns>A new Behavior who's value is computed by applying the lift function to the current
        /// behavior, and the given behaviors.</returns>
        /// <remarks>Lift converts a function on values to a Behavior on values</remarks>
        public IBehavior<TD> Lift<T, TB, TC, TD>(IBehavior<T> source, Func<T, TB, TC, TD> lift, IBehavior<TB> b, IBehavior<TC> c)
        {
            Func<T, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            var bf = source.Map(map);
            var l1 = b.Apply(bf);

            var result = c.Apply(l1);
            result.Register(bf);
            result.Register(l1);
            return result;
        }

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        /// <typeparam name="TB">The return type of the mapping function</typeparam>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source"> </param>
        /// <param name="map">The mapping function that converts from T -> TB</param>
        /// <returns>A new Behavior that updates whenever the current Behavior updates,
        /// having a value computed by the map function, and starting with the value
        /// of the current event mapped.</returns>
        public IBehavior<TB> Map<T, TB>(IBehavior<T> source, Func<T, TB> map)
        {
            var mapEvent = source.Source.Map(map);
            var behavior = mapEvent.Hold(map(source.Value));
            behavior.Register(mapEvent);
            return behavior;
        }

        /// <summary>
        /// Map firings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that fires whenever the current Event fires, the
        /// the mapped value is computed using the supplied mapping.</returns>
        public IEvent<TB> Map<T, TB>(IEvent<T> source, Func<T, TB> map)
        {
            return new MapEvent<T, TB>(source, map);
        }

        /// <summary>
        /// Merge two streams of events of the same type.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="observable">The Event to merge with the current Event</param>
        /// <returns>A new Event that fires whenever either the current or source Events fire.</returns>
        /// <remarks>
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event firings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </remarks>
        public IEvent<T> Merge<T>(IObservable<T> source, IObservable<T> observable)
        {
            return new MergeEvent<T>(source, observable);
        }

        /// <summary>
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="observable">The Event to merge with the current Event</param>
        /// <param name="coalesce">The coalesce function that combines simultaneous firings.</param>
        /// <returns>An Event that is fired whenever the current or source Events fire, where
        /// simultaneous firings are handled by the coalesce function.</returns>
        /// <remarks>
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        /// </remarks>
        public IEvent<T> Merge<T>(IObservable<T> source, IObservable<T> observable, Func<T, T, T> coalesce)
        {
            var merge = this.Merge(source, observable);
            var c = this.Coalesce(merge, coalesce);
            c.Register(merge);
            return c;
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <returns>An Event that only fires one time, the first time the current event fires.</returns>
        public IEvent<T> Once<T>(IObservable<T> source)
        {
            return new OnceEvent<T>(source);
        }

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="valueStream">The Behavior to sample when calculating the snapshot</param>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <returns>A new Event that will produce the snapshot when the current event fires</returns>
        public IEvent<TC> Snapshot<T, TB, TC>(IObservable<T> source, IValue<TB> valueStream, Func<T, TB, TC> snapshot)
        {
            return new SnapshotEvent<T, TB, TC>(source, snapshot, valueStream);
        }

        /// <summary>
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="valueStream">The Behavior to sample when taking the snapshot</param>
        /// <returns>An event that captures the Behaviors value when the current event fires</returns>
        public IEvent<TB> Snapshot<T, TB>(IObservable<T> source, IValue<TB> valueStream)
        {
            return this.Snapshot(source, valueStream, (a, b) => b);
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source"> </param>
        /// <param name="callback"> action to invoke when the underlying event fires</param>
        /// <returns>The event subscription</returns>
        /// <remarks>Immediately after creating the subscription, the callback will be fired with the 
        /// current value of the behavior.</remarks>
        public ISubscription<T> SubscribeValues<T>(IBehavior<T> source, Action<T> callback)
        {
            return SubscribeValues(source, new SodiumCallback<T>((a, t) => callback(a)), Rank.Highest);
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="source"> </param>
        /// <param name="callback"> action to invoke when the underlying event fires</param>
        /// <param name="rank">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>The event subscription</returns>
        /// <remarks>Immediately after creating the subscription, the callback will be fired with the 
        /// current value of the behavior.</remarks>
        public ISubscription<T> SubscribeValues<T>(IBehavior<T> source, ISodiumCallback<T> callback, Rank rank)
        {
            var evt = this.StartTransaction(t => new SubscribeFireLastValueEvent<T>(source, t));
            var subscription = (Subscription<T>)evt.Subscribe(callback, rank);
            subscription.Register(evt);
            return subscription;
        }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        /// <param name="source">The Behavior with an inner Behavior to unwrap.</param>
        /// <returns>The new, unwrapped Behavior</returns>
        /// <remarks>Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.</remarks>
        public IBehavior<T> SwitchB<T>(IBehavior<IBehavior<T>> source)
        {
            var initValue = source.Value.Value;
            var sink = new SwitchBehavior<T>(source);
            var result = sink.Hold(initValue);
            result.Register(sink);
            return result;
        }

        /// <summary>
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        /// </summary>
        /// <param name="behavior">The behavior that wraps the event</param>
        /// <returns>The unwrapped event</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// UnwrapEvent operation that takes a thread. This ensures that any other
        /// actions triggered during UnwrapEvent requiring a transaction all get the same instance.
        /// 
        /// Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.
        /// </remarks>
        public IEvent<T> SwitchE<T>(IBehavior<IEvent<T>> behavior)
        {
            return new SwitchEvent<T>(behavior);
        }
    }
}