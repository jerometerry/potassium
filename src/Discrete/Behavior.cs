namespace JT.Rx.Net.Discrete
{
    using System;
    using JT.Rx.Net.Core;

    /// <summary>
    /// EventBasedBehavior is a Behavior who's value is updated when the underlying Event is updated.
    /// </summary>
    /// <typeparam name="T">The type of values that will be published through the Behavior.</typeparam>
    public class Behavior<T> : DisposableObject, IBehavior<T>
    {
        private ObservedValue<T> observedValue;

        /// <summary>
        /// Create a behavior with a time varying value from an initial value
        /// </summary>
        /// <param name="value">The initial value of the Behavior</param>
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
            : this(source, value)
        {
            this.Source = source;
        }

        /// <summary>
        /// Constructs a new ObservableDrivenBehavior from an observable and a starting value
        /// </summary>
        /// <param name="observable">The Observable to monitor for updates</param>
        /// <param name="value">The initial value of the Behavior</param>
        private Behavior(Observable<T> observable, T value)
        {
            this.observedValue = new ObservedValue<T>(observable, value);
        }

        /// <summary>
        /// The underlying Event of the current Behavior
        /// </summary>
        public Event<T> Source { get; private set; }

        public T Value
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
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        /// <typeparam name="TB">The return type of the inner function of the given Behavior</typeparam>
        /// <param name="bf">Behavior of functions that maps from T -> TB</param>
        /// <returns>The new applied Behavior</returns>
        public Behavior<TB> Apply<TB>(Behavior<Func<T, TB>> bf)
        {
            var evt = new BehaviorApplyEvent<T, TB>(this, bf);
            var map = bf.Value;
            var valA = this.Value;
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
        /// Lift a binary function into behaviors.
        /// </summary>
        /// <typeparam name="TB">The type of the given Behavior</typeparam>
        /// <typeparam name="TC">The return type of the lift function.</typeparam>
        /// <param name="lift">The function to lift, taking a T and a TB, returning TC</param>
        /// <param name="b">The behavior used to apply a partial function by mapping the given 
        /// lift method to the current Behavior.</param>
        /// <returns>A new Behavior who's value is computed using the current Behavior, the given
        /// Behavior, and the lift function.</returns>
        public Behavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, Behavior<TB> b)
        {
            Func<T, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            var bf = this.Map(ffa);
            var result = b.Apply(bf);
            result.Register(bf);
            return result;
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        /// <typeparam name="TB">Type of Behavior b</typeparam>
        /// <typeparam name="TC">Type of Behavior c</typeparam>
        /// <typeparam name="TD">Return type of the lift function</typeparam>
        /// <param name="lift">The function to lift</param>
        /// <param name="b">Behavior of type TB used to do the lift</param>
        /// <param name="c">Behavior of type TC used to do the lift</param>
        /// <returns>A new Behavior who's value is computed by applying the lift function to the current
        /// behavior, and the given behaviors.</returns>
        /// <remarks>Lift converts a function on values to a Behavior on values</remarks>
        public Behavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, Behavior<TB> b, Behavior<TC> c)
        {
            Func<T, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            var bf = this.Map(map);
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