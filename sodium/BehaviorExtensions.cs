namespace Sodium
{
    using System;

    /// <summary>
    /// Reactive extensions for Behaviors
    /// </summary>
    public static class BehaviorExtensions
    {
        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">The return type of the inner function of the given Behavior</typeparam>
        /// <param name="source">The source behavior</param>
        /// <param name="bf">Behavior of functions that maps from T -> TB</param>
        /// <returns>The new applied Behavior</returns>
        public static DiscreteBehavior<TB> Apply<TA, TB>(this DiscreteBehavior<TA> source, DiscreteBehavior<Func<TA, TB>> bf)
        {
            var evt = new BehaviorApplyEvent<TA, TB>(source, bf);
            var map = bf.Value;
            var valA = source.Value;
            var valB = map(valA);
            var behavior = evt.Hold(valB);
            behavior.Register(evt);
            return behavior;
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
        public static Event<T> Coalesce<T>(this DiscreteBehavior<T> source, Func<T, T, T> coalesce)
        {
            return source.Source.Coalesce(coalesce);
        }

        /// <summary>
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">The type of the returned Behavior</typeparam>
        /// <typeparam name="TS">The snapshot function</typeparam>
        /// <param name="source"> </param>
        /// <param name="initState">Value to pass to the snapshot function</param>
        /// <param name="snapshot">Snapshot function</param>
        /// <returns>A new Behavior that collects values of type TB</returns>
        public static DiscreteBehavior<TB> Collect<TA, TB, TS>(this DiscreteBehavior<TA> source, TS initState, Func<TA, TS, Tuple<TB, TS>> snapshot)
        {
            var coalesceEvent = source.Coalesce((a, b) => b);
            var currentValue = source.Value;
            var tuple = snapshot(currentValue, initState);
            var loop = new EventFeed<Tuple<TB, TS>>();
            var loopBehavior = loop.Hold(tuple);
            var snapshotBehavior = loopBehavior.Map(x => x.Item2);
            var coalesceSnapshotEvent = coalesceEvent.Snapshot(snapshotBehavior, snapshot);
            loop.Feed(coalesceSnapshotEvent);

            var result = loopBehavior.Map(x => x.Item1);
            result.Register(loop);
            result.Register(loopBehavior);
            result.Register(coalesceEvent);
            result.Register(coalesceSnapshotEvent);
            result.Register(snapshotBehavior);
            return result;
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">The type of the given Behavior</typeparam>
        /// <typeparam name="TC">The return type of the lift function.</typeparam>
        /// <param name="a">The source Behavior</param>
        /// <param name="lift">The function to lift, taking a T and a TB, returning TC</param>
        /// <param name="b">The behavior used to apply a partial function by mapping the given 
        /// lift method to the current Behavior.</param>
        /// <returns>A new Behavior who's value is computed using the current Behavior, the given
        /// Behavior, and the lift function.</returns>
        public static DiscreteBehavior<TC> Lift<TA, TB, TC>(this DiscreteBehavior<TA> a, Func<TA, TB, TC> lift, DiscreteBehavior<TB> b)
        {
            Func<TA, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            var bf = a.Map(ffa);
            var result = b.Apply(bf);
            result.Register(bf);
            return result;
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">Type of Behavior b</typeparam>
        /// <typeparam name="TC">Type of Behavior c</typeparam>
        /// <typeparam name="TD">Return type of the lift function</typeparam>
        /// <param name="a">The source Behavior</param>
        /// <param name="lift">The function to lift</param>
        /// <param name="b">Behavior of type TB used to do the lift</param>
        /// <param name="c">Behavior of type TC used to do the lift</param>
        /// <returns>A new Behavior who's value is computed by applying the lift function to the current
        /// behavior, and the given behaviors.</returns>
        /// <remarks>Lift converts a function on values to a Behavior on values</remarks>
        public static DiscreteBehavior<TD> Lift<TA, TB, TC, TD>(this DiscreteBehavior<TA> a, Func<TA, TB, TC, TD> lift, DiscreteBehavior<TB> b, DiscreteBehavior<TC> c)
        {
            Func<TA, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            var bf = a.Map(map);
            var l1 = b.Apply(bf);

            var result = c.Apply(l1);
            result.Register(bf);
            result.Register(l1);
            return result;
        }

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">The return type of the mapping function</typeparam>
        /// <param name="source">The source Behavior</param>
        /// <param name="map">The mapping function that converts from T -> TB</param>
        /// <returns>A new Behavior that updates whenever the current Behavior updates,
        /// having a value computed by the map function, and starting with the value
        /// of the current event mapped.</returns>
        public static DiscreteBehavior<TB> Map<TA, TB>(this DiscreteBehavior<TA> source, Func<TA, TB> map)
        {
            var mapEvent = source.Source.Map(map);
            var behavior = mapEvent.Hold(map(source.Value));
            behavior.Register(mapEvent);
            return behavior;
        }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The Behavior with an inner Behavior to unwrap.</param>
        /// <returns>The new, unwrapped Behavior</returns>
        /// <remarks>Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.</remarks>
        public static DiscreteBehavior<T> Switch<T>(this DiscreteBehavior<DiscreteBehavior<T>> source)
        {
            var value = source.Value.Value;
            var sink = new SwitchBehaviorEvent<T>(source);
            var result = sink.Hold(value);
            result.Register(sink);
            return result;
        }

        /// <summary>
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="behavior">The behavior that wraps the event</param>
        /// <returns>The unwrapped event</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// UnwrapEvent operation that takes a thread. This ensures that any other
        /// actions triggered during UnwrapEvent requiring a transaction all get the same instance.
        /// 
        /// Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.
        /// </remarks>
        public static Event<T> Switch<T>(this DiscreteBehavior<Event<T>> behavior)
        {
            return new SwitchEvent<T>(behavior);
        }

        /// <summary>
        /// Get an Event that publishes the initial Behaviors value, and whenever the Behaviors value changes.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The source Behavior</param>
        /// <returns>The event streams</returns>
        public static Event<T> Values<T>(this DiscreteBehavior<T> source)
        {
            return Transaction.Start(t => new BehaviorLastValueEvent<T>(source, t));
        }
    }
}
