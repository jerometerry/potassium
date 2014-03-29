namespace Potassium.Core
{
    using System;
    using Potassium.Core;

    /// <summary>
    /// Event extension methods
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        /// <typeparam name="T">The type of the source Event</typeparam>
        /// <typeparam name="TS">The return type of the snapshot function</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="value">The initial state of the behavior</param>
        /// <param name="accumulator">The snapshot generation function</param>
        /// <returns>A new Behavior starts with the given initial value, and who's value
        /// is computed using the old value and the accumulator function.</returns>
        public static Behavior<TS> Accum<T, TS>(this Event<T> source, Func<T, TS, TS> accumulator, TS value)
        {
            EventRepeater<TS> repeater = new EventRepeater<TS>();

            // Behavior holds the running snapshot value
            Behavior<TS> previousShapshotBehavior = repeater.Hold(value);

            // Event that fires the new accumulated values, using the accumulator and the previous values
            Event<TS> accumulationEvent = source.Snapshot(accumulator, previousShapshotBehavior);

            // Repeat the new accumulated values into the Behavior, to store the new snapshot as the previous snapshot
            repeater.Repeat(accumulationEvent);

            // Behavior that holds the value of the new accumulated values
            Behavior<TS> result = accumulationEvent.Hold(value);

            result.Register(repeater);
            result.Register(previousShapshotBehavior);
            result.Register(accumulationEvent);

            return result;
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="T">The type of the source Event</typeparam>
        /// <typeparam name="TB">The return type of the new Event</typeparam>
        /// <typeparam name="TS">The snapshot type</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="initState">The initial state for the internal Behavior</param>
        /// <param name="collector">The mealy machine</param>
        /// <returns>An Event that collects new values</returns>
        /// <remarks>Collect is similar to Accum, except the return value is an Event that fires
        /// the collected value, and the collect function computes two values instead of one -
        /// the new collected value, and the new snapshot that can be used to compute the
        /// next collected value.</remarks>
        public static Event<TB> Collect<T, TB, TS>(this Event<T> source, Func<T, TS, Tuple<TB, TS>> collector, TS initState)
        {
            // snapshotRepeater is used to create the Behavior that holds the snapshot values
            EventRepeater<TS> snapshotRepeater = new EventRepeater<TS>();

            // Behavior that holds the previous collected value
            Behavior<TS> snapshotBehavior = snapshotRepeater.Hold(initState);

            // Event that emits a Tuple<TB,TS> containing the mapped value and the snapshot
            Event<Tuple<TB, TS>> mappedEventSnapshot = source.Snapshot(collector, snapshotBehavior);

            // Event that emits the snapshot values from the mappedEventSnapshot above
            Event<TS> snapshotEvent = mappedEventSnapshot.Map(bs => bs.Item2);

            // Repeat the snapshots into the Behavior holding the snapshot values
            snapshotRepeater.Repeat(snapshotEvent);

            // Event that extracts the mapped value from the mappedEventSnapshot above
            Event<TB> mappedEvent = mappedEventSnapshot.Map(bs => bs.Item1);

            mappedEvent.Register(snapshotRepeater);
            mappedEvent.Register(snapshotBehavior);
            mappedEvent.Register(mappedEventSnapshot);
            mappedEvent.Register(snapshotEvent);

            return mappedEvent;
        }
    }
}
