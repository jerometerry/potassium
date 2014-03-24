namespace Potassium.Core
{
    using System;
    using Potassium.Internal;
    using Potassium.Providers;

    /// <summary>
    /// Behavior contains a value which is updated when the underlying Event is updated.
    /// </summary>
    /// <typeparam name="T">The type of values that will be published through the Behavior.</typeparam>
    public class Behavior<T> : Provider<T>
    {
        private ObservedValue<T> observedValue;

        /// <summary>
        /// Creates a constant Behavior
        /// </summary>
        /// <param name="value">The value of the Behavior</param>
        public Behavior(T value)
            : this(value, new Event<T>())
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Create a behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Observable to listen for updates from</param>
        /// <param name="value">The initial value of the Behavior</param>
        public Behavior(T value, Event<T> source)
        {
            this.Source = source;
            this.observedValue = Transaction.Start(t => new ObservedValue<T>(value, source, t));
        }

        /// <summary>
        /// The underlying Event of the current Behavior
        /// </summary>
        public Event<T> Source { get; private set; }

        public override T Value
        {
            get
            {
                return observedValue.Value;
            }
        }

        /// <summary>
        /// New value of the Behavior that will be posted to Value when the Transaction completes
        /// </summary>
        internal T NewValue
        {
            get
            {
                return this.observedValue.NewValue;
            }
        }

        /// <summary>
        /// Lift a constant into a Behavior
        /// </summary>
        /// <param name="value">The value to lift into a Behavior</param>
        /// <returns>A constant Behavior, having the given value</returns>
        public static Behavior<T> Lift(T value)
        {
            return new Behavior<T>(value);
        }

        /// <summary>
        /// Lift a unary function into a Behavior
        /// </summary>
        /// <typeparam name="TB">The return type of the lift function</typeparam>
        /// <param name="lift">The function to lift</param>
        /// <param name="a">The behavior to supply as a parameter to the lift function</param>
        /// <returns>The lifted Behavior</returns>
        public static Behavior<TB> Lift<TB>(Func<T, TB> lift, Behavior<T> a)
        {
            return a.Map(lift);
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        /// <typeparam name="TB">The type of the given Behavior</typeparam>
        /// <typeparam name="TC">The return type of the lift function.</typeparam>
        /// <param name="lift">The function to lift, taking a T and a TB, returning TC</param>
        /// <param name="a">Behavior who's value will be used as the first parameter to the lift function</param>
        /// <param name="b">Behavior who's value will be used as the second parameter to the lift function</param>
        /// <returns>A new Behavior who's value is computed using the current Behavior, the given
        /// Behavior, and the lift function.</returns>
        public static Behavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, Behavior<T> a, Behavior<TB> b)
        {
            Func<T, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            Behavior<Func<TB, TC>> partialB = Lift(ffa, a);
            Behavior<TC> behaviorC = Behavior<TB>.Curry(partialB, b);
            behaviorC.Register(partialB);
            return behaviorC;
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        /// <typeparam name="TB">Type of Behavior b</typeparam>
        /// <typeparam name="TC">Type of Behavior c</typeparam>
        /// <typeparam name="TD">Return type of the lift function</typeparam>
        /// <param name="lift">The function to lift</param>
        /// <param name="a">Behavior who's value will be used as the first parameter to the lift function</param>
        /// <param name="b">Behavior who's value will be used as the second parameter to the lift function</param>
        /// <param name="c">Behavior who's value will be used as the third parameter to the lift function</param>
        /// <returns>A new Behavior who's value is computed by applying the lift function to the current
        /// behavior, and the given behaviors.</returns>
        /// <remarks>Lift converts a function on values to a Behavior on values</remarks>
        public static Behavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, Behavior<T> a, Behavior<TB> b, Behavior<TC> c)
        {
            Func<T, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            Behavior<Func<TB, Func<TC, TD>>> partialB = Lift(map, a);
            Behavior<Func<TC, TD>> partialC = Behavior<TB>.Curry(partialB, b);
            Behavior<TD> behaviorD = Behavior<TC>.Curry(partialC, c);
            behaviorD.Register(partialB);
            behaviorD.Register(partialC);
            return behaviorD;
        }

        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        /// <typeparam name="TB">The return type of the inner function of the given Behavior</typeparam>
        /// <param name="partialBehavior">Behavior of functions that maps from T -> TB</param>
        /// <param name="a">Parameter to supply to the partial function</param>
        /// <returns>The new applied Behavior</returns>
        public static Behavior<TB> Curry<TB>(Behavior<Func<T, TB>> partialBehavior, Behavior<T> a)
        {
            var evt = new CurryEvent<T, TB>(partialBehavior, a);
            var map = partialBehavior.Value;
            var valA = a.Value;
            var valB = map(valA);
            var behavior = evt.Hold(valB);
            behavior.Register(evt);
            return behavior;
        }

        /// <summary>
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="TB">The type of the returned Behavior</typeparam>
        /// <typeparam name="TS">The snapshot function</typeparam>
        /// <param name="initState">Value to pass to the snapshot function</param>
        /// <param name="snapshot">Snapshot function</param>
        /// <returns>A new Behavior that collects values of type TB</returns>
        public Behavior<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var coalesceEvent = this.Source.Coalesce((a, b) => b);
            var currentValue = this.Value;
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
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        /// <typeparam name="TB">The return type of the mapping function</typeparam>
        /// <param name="map">The mapping function that converts from T -> TB</param>
        /// <returns>A new Behavior that updates whenever the current Behavior updates,
        /// having a value computed by the map function, and starting with the value
        /// of the current event mapped.</returns>
        public Behavior<TB> Map<TB>(Func<T, TB> map)
        {
            var mapEvent = this.Source.Map(map);
            var behavior = mapEvent.Hold(map(this.Value));
            behavior.Register(mapEvent);
            return behavior;
        }

        /// <summary>
        /// Get an Event that publishes the initial Behaviors value, and whenever the Behaviors value changes.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <returns>The event streams</returns>
        public Event<T> Values()
        {
            return Transaction.Start(t => new BehaviorLastValueEvent<T>(this, t));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                observedValue.Dispose();
                observedValue = null;
            }

            base.Dispose(disposing);
        }
    }
}