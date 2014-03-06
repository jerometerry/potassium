namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a time varying value. It starts with an initial value which 
    /// gets updated as the underlying Event is fired.
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the Behavior.</typeparam>
    public class Behavior<T> : Observable<T>
    {
        /// <summary>
        /// Holding tank for updates from the underlying Event, waiting to be 
        /// moved into the current value of the Behavior.
        /// </summary>
        private Maybe<T> valueUpdate = Maybe<T>.Null;

        /// <summary>
        /// Listener that listens for firings from the underlying Event.
        /// </summary>
        private IEventListener<T> valueUpdateListener;

        /// <summary>
        /// A constant behavior
        /// </summary>
        /// <param name="initValue">The initial value of the Behavior</param>
        public Behavior(T initValue)
            : this(new Event<T>(), initValue)
        {
            this.RegisterFinalizer(this.Source);
        }

        /// <summary>
        /// Create A behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Event to listen for updates from</param>
        /// <param name="initValue">The initial value of the Behavior</param>
        public Behavior(Event<T> source, T initValue)
        {
            this.Source = source;
            this.Value = initValue;
            this.valueUpdateListener = ListenForEventFirings();
        }

        /// <summary>
        /// The underlying event that gives the updates for the behavior. If this behavior was created
        /// with a hold, then Source gives you an event equivalent to the one that was held.
        /// </summary>
        public Event<T> Source { get; private set; }

        /// <summary>
        /// Sample the behavior's current value.
        /// </summary>
        /// <remarks>
        /// This should generally be avoided in favor of GetValueStream().Listen(..) so you don't
        /// miss any updates, but in many circumstances it makes sense.
        ///
        /// It can be best to use it inside an explicit transaction (using TransactionContext.Current.Run()).
        /// For example, a b.Value inside an explicit transaction along with a
        /// b.Source.Listen(..) will capture the current value and any updates without risk
        /// of missing any in between.
        /// </remarks>
        public T Value { get; private set; }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        /// <param name="source">The Behavior with an inner Behavior to unwrap.</param>
        /// <returns>The new, unwrapped Behavior</returns>
        /// <remarks>Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.</remarks>
        public static Behavior<T> SwitchB(Behavior<Behavior<T>> source)
        {
            var innerBehavior = source.Value;
            var initValue = innerBehavior.Value;
            var sink = new SwitchBehaviorEvent<T>(source);
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
        /// reactive logic to modify reactive logic.>
        /// </remarks>
        public static Event<T> SwitchE(Behavior<Event<T>> behavior)
        {
            return new SwitchEvent<T>(behavior);
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
            var evt = new BehaviorApplyEvent<T, TB>(bf, this);
            var behavior = evt.Behavior;
            behavior.RegisterFinalizer(evt);
            return behavior;
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the underlying event fires</param>
        /// <returns>The event listener</returns>
        public override IEventListener<T> Listen(Action<T> callback)
        {
            return this.Source.Listen(callback);
        }

        /// <summary>
        /// An event that is guaranteed to fire once when you listen to it, giving
        /// the current value of the behavior, and thereafter behaves like updates(),
        /// firing for each update to the behavior's value.
        /// </summary>
        /// <returns>An event that will fire when it's listened to, and every time it's 
        /// value changes thereafter</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// Value operation that takes a thread. This ensures that any other
        /// actions triggered during Value requiring a transaction all get the same instance.</remarks>
        public Event<T> Values()
        {
            return this.StartTransaction(this.Values);
        }

        /// <summary>
        /// Gets the updated value of the Behavior that has not yet been moved to the
        /// current value of the Behavior. 
        /// </summary>
        /// <returns>
        /// The updated value, if any, otherwise the current value
        /// </returns>
        /// <remarks>As the underlying event is fired, the current behavior is 
        /// updated with the value of the firing. However, it doesn't go directly to the
        /// value field. Instead, the value is put into newValue, and a Last Action is
        /// scheduled to move the value from newValue to value.</remarks>
        public T GetNewValue()
        {
            return valueUpdate.HasValue ? valueUpdate.Value() : Value;
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
            var underlyingEvent = Source;
            var mapEvent = underlyingEvent.Map(map);
            var currentValue = Value;
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
        public Behavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, Behavior<TB> behavior)
        {
            Func<T, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            var bf = Map(ffa);
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
        public Behavior<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var coalesceEvent = Source.Coalesce((a, b) => b);
            var currentValue = Value;
            var tuple = snapshot(currentValue, initState);
            var loop = new EventLoop<Tuple<TB, TS>>();
            var loopBehavior = loop.Hold(tuple);
            var snapshotBehavior = loopBehavior.Map(x => x.Item2);
            var coalesceSnapshotEvent = coalesceEvent.Snapshot(snapshotBehavior, snapshot);
            loop.Loop(coalesceSnapshotEvent);

            var result = loopBehavior.Map(x => x.Item1);
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
        public Behavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, Behavior<TB> b, Behavior<TC> c)
        {
            Func<T, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            var bf = Map(map);
            var l1 = b.Apply(bf);

            var result = c.Apply(l1);
            result.RegisterFinalizer(bf);
            result.RegisterFinalizer(l1);
            return result;
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the underlying event fires</param>
        /// <param name="rank">The rank of the action, used as a superior to the rank of the underlying action.</param>
        /// <returns>The event listener</returns>
        /// <remarks>Lift converts a function on values to a Behavior on values</remarks>
        internal IEventListener<T> Listen(ISodiumCallback<T> callback, Rank rank)
        {
            return this.Source.Listen(callback, rank);
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the underlying event fires</param>
        /// <param name="rank">The rank of the action, used as a superior to the rank of the underlying action.</param>
        /// <param name="transaction">The transaction used to order actions</param>
        /// <returns>The event listener</returns>
        internal IEventListener<T> Listen(ISodiumCallback<T> callback, Rank rank, Transaction transaction)
        {
            return this.Source.Listen(callback, rank, transaction);
        }

        /// <summary>
        /// An event that is guaranteed to fire once when you listen to it, giving
        /// the current value of the behavior, and thereafter behaves like updates(),
        /// firing for each update to the behavior's value.
        /// </summary>
        /// <param name="transaction">The transaction to run the Value operation on</param>
        /// <returns>An event that will fire when it's listened to, and every time it's 
        /// value changes thereafter</returns>
        internal Event<T> Values(Transaction transaction)
        {
            var valueEvent = new BehaviorValueEvent<T>(this, transaction);

            // Needed in case of an initial value and an update
            // in the same transaction.
            var result = new LastFiringEvent<T>(valueEvent, transaction);
            result.RegisterFinalizer(valueEvent);
            return result;
        }

        /// <summary>
        /// Dispose of the current Behavior
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (this.valueUpdateListener != null)
            {
                this.valueUpdateListener.Dispose();
                this.valueUpdateListener = null;
            }

            this.Source = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <returns>The IEventListener registered with the underlying event.</returns>
        private IEventListener<T> ListenForEventFirings()
        {
            return this.StartTransaction(this.ListenForEventFirings);
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <param name="transaction">The transaction to schedule the listen on.</param>
        /// <returns>The IEventListener registered with the underlying event.</returns>
        private IEventListener<T> ListenForEventFirings(Transaction transaction)
        {
            var callback = new ActionCallback<T>(ScheduleApplyValueUpdate);
            var result = this.Listen(callback, Rank.Highest, transaction);
            return result;
        }

        /// <summary>
        /// Store the updated value, and schedule a Transaction.Last action that will move the updated value 
        /// into the current value.
        /// </summary>
        /// <param name="transaction">The transaction to schedule the value update on</param>
        /// <param name="update">The updated value</param>
        private void ScheduleApplyValueUpdate(T update, Transaction transaction)
        {
            if (!valueUpdate.HasValue)
            {
                transaction.Medium(ApplyValueUpdate);
            }

            valueUpdate = new Maybe<T>(update);
        }

        /// <summary>
        /// Store the updated value into the current value, and set the updated value to null.
        /// </summary>
        private void ApplyValueUpdate()
        {
            Value = valueUpdate.Value();
            valueUpdate = Maybe<T>.Null;
        }
    }
}