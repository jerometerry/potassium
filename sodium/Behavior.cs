namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a time varying value. It starts with an initial value which 
    /// gets updated as the underlying Event is fired.
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the Behavior.</typeparam>
    public class Behavior<T> : EventLoop<T>, IBehavior<T>
    {
        /// <summary>
        /// A constant behavior
        /// </summary>
        /// <param name="initValue">The initial value of the Behavior</param>
        public Behavior(T initValue)
        {
            this.ValueContainer = new ValueContainer<T>(initValue);
        }

        /// <summary>
        /// Create A behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Event to listen for updates from</param>
        /// <param name="initValue">The initial value of the Behavior</param>
        public Behavior(IEvent<T> source, T initValue)
        {
            this.Loop(source);
            this.ValueContainer = new ValueContainer<T>(this, initValue);
        }

        /// <summary>
        /// Sample the behavior's current value.
        /// </summary>
        /// <remarks>
        /// This should generally be avoided in favor of GetValueStream().Subscribe(..) so you don't
        /// miss any updates, but in many circumstances it makes sense.
        ///
        /// It can be best to use it inside an explicit transaction (using TransactionContext.Current.Run()).
        /// For example, a b.Value inside an explicit transaction along with a
        /// b.Source.Subscribe(..) will capture the current value and any updates without risk
        /// of missing any in between.
        /// </remarks>
        public T Value
        {
            get
            {
                return this.ValueContainer.Value;
            }
        }

        internal ValueContainer<T> ValueContainer { get; private set; }

        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        /// <typeparam name="TS">The return type of the snapshot function</typeparam>
        /// <param name="source">The source snapshot</param>
        /// <param name="initState">The initial state of the behavior</param>
        /// <param name="snapshot">The snapshot generation function</param>
        /// <returns>A new Behavior starting with the given value, that updates 
        /// whenever the current event fires, getting a value computed by the snapshot function.</returns>
        public static IBehavior<TS> Accum<TS>(IEvent<T> source, TS initState, Func<T, TS, TS> snapshot)
        {
            var evt = new EventLoop<TS>();
            var behavior = evt.Hold(initState);

            var snapshotEvent = source.Snapshot(behavior, snapshot);
            evt.Loop(snapshotEvent);

            var result = snapshotEvent.Hold(initState);
            result.RegisterFinalizer(evt);
            result.RegisterFinalizer(behavior);
            result.RegisterFinalizer(snapshotEvent);

            return result;
        }

        /// <summary>
        /// Creates a Behavior from an Observable and an initial value
        /// </summary>
        /// <param name="source">The source to update the Behavior from</param>
        /// <param name="initValue">The initial value of the Behavior</param>
        /// <returns>The Behavior with the given value</returns>
        public static IBehavior<T> Hold(IEvent<T> source, T initValue)
        {
            return TransactionContext.Current.Run(t => Hold(source, initValue, t));
        }

        /// <summary>
        /// Creates a Behavior from an Observable and an initial value
        /// </summary>
        /// <param name="source">The source to update the Behavior from</param>
        /// <param name="initValue">The initial value of the Behavior</param>
        /// <param name="t">The Transaction to perform the Hold</param>
        /// <returns>The Behavior with the given value</returns>
        public static IBehavior<T> Hold(IEvent<T> source, T initValue, Transaction t)
        {
            var s = new LastFiring<T>(source, t);
            var b = new Behavior<T>(s, initValue);
            b.RegisterFinalizer(s);
            return b;
        }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        /// <param name="source">The Behavior with an inner Behavior to unwrap.</param>
        /// <returns>The new, unwrapped Behavior</returns>
        /// <remarks>Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.</remarks>
        public static IBehavior<T> SwitchB(IBehavior<IBehavior<T>> source)
        {
            var innerBehavior = source.Value;
            var initValue = innerBehavior.Value;
            var sink = new SwitchBehavior<T>(source);
            var result = sink.Hold(initValue);
            result.RegisterFinalizer(sink);
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
        public static IEvent<T> SwitchE(IBehavior<IEvent<T>> behavior)
        {
            return new Switch<T>(behavior);
        }

        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        /// <typeparam name="TB">The return type of the inner function of the given Behavior</typeparam>
        /// <param name="bf">Behavior of functions that maps from T -> TB</param>
        /// <returns>The new applied Behavior</returns>
        public IBehavior<TB> Apply<TB>(IBehavior<Func<T, TB>> bf)
        {
            var evt = new BehaviorApply<T, TB>((Behavior<Func<T, TB>>)bf, this);
            var behavior = evt.Behavior;
            behavior.RegisterFinalizer(evt);
            return behavior;
        }

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        /// <typeparam name="TB">The return type of the mapping function</typeparam>
        /// <param name="map">The mapping function that converts from T -> TB</param>
        /// <returns>A new Behavior that updates whenever the current Behavior updates,
        /// having a value computed by the map function, and starting with the value
        /// of the current event mapped.</returns>
        public IBehavior<TB> MapB<TB>(Func<T, TB> map)
        {
            var underlyingEvent = this;
            var mapEvent = underlyingEvent.Map(map);
            var currentValue = this.ValueContainer.Value;
            var mappedValue = map(currentValue);
            var behavior = mapEvent.Hold(mappedValue);
            behavior.RegisterFinalizer(mapEvent);
            return behavior;
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        /// <typeparam name="TB">The type of the given Behavior</typeparam>
        /// <typeparam name="TC">The return type of the lift function.</typeparam>
        /// <param name="lift">The function to lift, taking a T and a TB, returning TC</param>
        /// <param name="behavior">The behavior used to apply a partial function by mapping the given 
        /// lift method to the current Behavior.</param>
        /// <returns>A new Behavior who's value is computed using the current Behavior, the given
        /// Behavior, and the lift function.</returns>
        public IBehavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, IBehavior<TB> behavior)
        {
            Func<T, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            var bf = this.MapB(ffa);
            var result = behavior.Apply(bf);
            result.RegisterFinalizer(bf);
            return result;
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
        public IBehavior<TB> CollectB<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var coalesceEvent = this.Coalesce((a, b) => b);
            var currentValue = this.ValueContainer.Value;
            var tuple = snapshot(currentValue, initState);
            var loop = new EventLoop<Tuple<TB, TS>>();
            var loopBehavior = loop.Hold(tuple);
            var snapshotBehavior = loopBehavior.MapB(x => x.Item2);
            var coalesceSnapshotEvent = coalesceEvent.Snapshot(snapshotBehavior, snapshot);
            loop.Loop(coalesceSnapshotEvent);

            var result = loopBehavior.MapB(x => x.Item1);
            result.RegisterFinalizer(loop);
            result.RegisterFinalizer(loopBehavior);
            result.RegisterFinalizer(coalesceEvent);
            result.RegisterFinalizer(coalesceSnapshotEvent);
            result.RegisterFinalizer(snapshotBehavior);
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
        public IBehavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, IBehavior<TB> b, IBehavior<TC> c)
        {
            Func<T, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            var bf = this.MapB(map);
            var l1 = b.Apply(bf);

            var result = c.Apply(l1);
            result.RegisterFinalizer(bf);
            result.RegisterFinalizer(l1);
            return result;
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback"> action to invoke when the underlying event fires</param>
        /// <returns>The event subscription</returns>
        /// <remarks>Immediately after creating the subscription, the callback will be fired with the 
        /// current value of the behavior.</remarks>
        public ISubscription<T> SubscribeAndFire(Action<T> callback)
        {
            return this.SubscribeAndFire(new SodiumCallback<T>((a, t) => callback(a)), Rank.Highest);
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback"> action to invoke when the underlying event fires</param>
        /// <param name="rank">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>The event subscription</returns>
        /// <remarks>Immediately after creating the subscription, the callback will be fired with the 
        /// current value of the behavior.</remarks>
        public ISubscription<T> SubscribeAndFire(ISodiumCallback<T> callback, Rank rank)
        {
            var beh = this;
            var v = this.StartTransaction(t => new ValuesListFiring<T>(beh, t));
            var s = (Subscription<T>)v.Subscribe(callback, rank);
            s.RegisterFinalizer(v);
            return s;
        }

        /// <summary>
        /// Dispose of the current Behavior
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (this.ValueContainer != null)
            {
                this.ValueContainer.Dispose();
                this.ValueContainer = null;
            }

            base.Dispose(disposing);
        }
    }
}