namespace Potassium.Core
{
    using System;
    using Potassium.Core;
    using Potassium.Internal;    

    /// <summary>
    /// Behavior extensions methods
    /// </summary>
    public static class BehaviorExtensions
    {
        /// <summary>
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="T">The type of the source Behavior</typeparam>
        /// <typeparam name="TB">The type of the returned Behavior</typeparam>
        /// <typeparam name="TS">The snapshot function</typeparam>
        /// <param name="behavior">The source Behavior</param>
        /// <param name="initState">Value to pass to the snapshot function</param>
        /// <param name="snapshot">Snapshot function</param>
        /// <returns>A new Behavior that collects values of type TB</returns>
        public static Behavior<TB> Collect<T, TB, TS>(this Behavior<T> behavior, Func<T, TS, Tuple<TB, TS>> snapshot, TS initState)
        {
            // Only listen for the last firing of the Source of the Current Behavior
            Event<T> lastFiring = behavior.LastFiring();

            // Event that fires whenever a new snapshot is generated
            EventFeed<Tuple<TB, TS>> snapshotFeed = new EventFeed<Tuple<TB, TS>>();

            // Behavior that holds the last snapshot tuple
            Behavior<Tuple<TB, TS>> snapshotBehavior = snapshotFeed.Hold(snapshot(behavior.Value, initState));

            // Behavior that holds the last snapshot value, extracted out of the tuple
            Behavior<TS> lastSnapshotValue = snapshotBehavior.Map(x => x.Item2);

            // Takes snapshots from the last firing of the Source of the current Behavior
            Event<Tuple<TB, TS>> snapshotGenerator = lastFiring.Snapshot(snapshot, lastSnapshotValue);

            // Feed the new snapshots back into the snapshot feed.
            snapshotFeed.Feed(snapshotGenerator);

            // Extracts the value out of the snapshot Behavior tuple
            var collected = snapshotBehavior.Map(x => x.Item1);

            collected.Register(snapshotFeed);
            collected.Register(snapshotBehavior);
            collected.Register(lastFiring);
            collected.Register(snapshotGenerator);
            collected.Register(lastSnapshotValue);

            return collected;
        }

        /// <summary>
        /// Lifts a value into a constant Behavior
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="value">The value of the constant Behavior</param>
        /// <returns>A constant Behavior having the given value</returns>
        public static Behavior<T> Lift<T>(this T value)
        {
            return Functor.Lift(value);
        }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        /// <typeparam name="T">The type of values fired through the source</typeparam>
        /// <param name="source">The Behavior with an inner Behavior to unwrap.</param>
        /// <returns>The new, unwrapped Behavior</returns>
        /// <remarks>Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.</remarks>
        public static Behavior<T> Switch<T>(this Behavior<Behavior<T>> source)
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
        /// <typeparam name="T">The type of values fired through the source</typeparam>
        /// <param name="behavior">The behavior that wraps the event</param>
        /// <returns>The unwrapped event</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// UnwrapEvent operation that takes a thread. This ensures that any other
        /// actions triggered during UnwrapEvent requiring a transaction all get the same instance.
        /// 
        /// Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.
        /// </remarks>
        public static Event<T> Switch<T>(this Behavior<Event<T>> behavior)
        {
            return Transaction.Start(t => new SwitchEvent<T>(behavior, t));
        }
    }
}
