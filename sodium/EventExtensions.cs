namespace Sodium
{
    using System;

    /// <summary>
    /// Reactive extensions for Events
    /// </summary>
    public static class EventExtensions
    {
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
        public static Event<TB> Collect<TA, TB, TS>(this Event<TA> source, TS initState, Func<TA, TS, Tuple<TB, TS>> snapshot)
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
        /// Map publishings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TA">The type of values published through the source</typeparam>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that publishes whenever the current Event publishes, the
        /// the mapped value is computed using the supplied mapping.</returns>
        public static Event<TB> Map<TA, TB>(this Event<TA> source, Func<TA, TB> map)
        {
            return new MapEvent<TA, TB>(source, map);
        }
    }
}
