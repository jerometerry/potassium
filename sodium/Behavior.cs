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
        /// The current value of the Behavior, updated after the underlying event fires.
        /// </summary>
        private T value;
        
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
        /// A behavior with a time varying value
        /// </summary>
        /// <param name="initValue"></param>
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
            this.value = initValue;
            this.valueUpdateListener = ListenForEventFirings();
        }

        /// <summary>
        /// The underlying Event that the Value of the current Behavior is updated with.
        /// </summary>
        protected Event<T> Source { get; private set; }

        /// <summary>
        /// A behavior with a constant value.
        /// </summary>
        public static Behavior<T> Constant(T initValue)
        {
            var behavior = new Behavior<T>(new StaticEvent<T>(), initValue);
            behavior.RegisterFinalizer(behavior.Source);
            return behavior;
        }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        public static Behavior<T> Unwrap(Behavior<Behavior<T>> source)
        {
            var innerBehavior = source.Sample();
            var initValue = innerBehavior.Sample();
            var sink = new SwitchBehaviorEvent<T>(source);
            var result = sink.ToBehavior(initValue);
            result.RegisterFinalizer(sink);
            return result;
        }

        /// <summary>
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        /// </summary>
        /// <param name="behavior">The behavior that wraps the event</param>
        /// <returns>The unwrapped event</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// SwitchE operation that takes a thread. This ensures that any other
        /// actions triggered during SwitchE requiring a transaction all get the same instance.</remarks>
        public static Event<T> UnwrapEvent(Behavior<Event<T>> behavior)
        {
            return new SwitchEvent<T>(behavior);
        }

        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        public Behavior<TB> Apply<TB>(Behavior<Func<T, TB>> bf)
        {
            var evt = new BehaviorApplyEvent<T, TB>(bf, this);
            var behavior = evt.Behavior;
            behavior.RegisterFinalizer(evt);
            return behavior;
        }

        /// <summary>
        /// Dispose of the current Behavior
        /// </summary>
        public override void Dispose()
        {
            if (this.valueUpdateListener != null)
            {
                this.valueUpdateListener.Dispose();
                this.valueUpdateListener = null;
            }

            this.Source = null;
            
            base.Dispose();
        }

        /// <summary>
        /// Fire the given value to all registered listeners 
        /// </summary>
        /// <param name="firing">The value to be fired</param>
        public override bool Fire(T firing)
        {
            return this.Source.Fire(firing);
        }

        /// <summary>
        /// Fires the given value to all registered listeners, using the given transaction
        /// </summary>
        /// <param name="firing">The value to fire.</param>
        /// <param name="transaction">The transaction to used to order the firings</param>
        public override bool Fire(T firing, Transaction transaction)
        {
            return this.Source.Fire(firing, transaction);
        }

        /// <summary>
        /// Listen to the current Behavior for updates, which fires immediate with the 
        /// Behaviors current value, and every time the underlying event fires.
        /// </summary>
        /// <param name="callback">The action to take when the Behavior's underlying event fires</param>
        /// <returns>The listener</returns>
        public override IEventListener<T> Listen(Action<T> callback)
        {
            var v = this.Value();
            var l = (EventListener<T>)v.Listen(callback);
            l.RegisterFinalizer(v);
            return l;
        }

        /// <summary>
        /// Listen to the current Behavior for updates, which fires immediate with the 
        /// Behaviors current value, and every time the underlying event fires.
        /// </summary>
        /// <param name="callback">The action to take when the Behavior's underlying event fires</param>
        /// <returns>The listener</returns>
        public override IEventListener<T> Listen(ISodiumCallback<T> callback)
        {
            var v = this.Value();
            var l = (EventListener<T>)v.Listen(callback);
            l.RegisterFinalizer(v);
            return l;
        }

        /// <summary>
        /// Listen to the current Behavior for updates, but don't fire the initial value
        /// </summary>
        /// <param name="callback">The action to take when the Behavior's underlying event fires</param>
        /// <returns>The listener</returns>
        public override IEventListener<T> ListenSuppressed(Action<T> callback)
        {
            var v = this.Updates();
            var l = (EventListener<T>)v.Listen(callback);
            l.RegisterFinalizer(v);
            return l;
        }

        /// <summary>
        /// Listen to the current Behavior for updates, but don't fire the initial value
        /// </summary>
        /// <param name="callback">The action to take when the Behavior's underlying event fires</param>
        /// <returns>The listener</returns>
        public override IEventListener<T> ListenSuppressed(ISodiumCallback<T> callback)
        {
            var v = this.Updates();
            var l = (EventListener<T>)v.Listen(callback);
            l.RegisterFinalizer(v);
            return l;
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
        public Event<T> Value()
        {
            return this.Run<Event<T>>(Value);
        }

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        public Behavior<TB> Map<TB>(Func<T, TB> map)
        {
            var underlyingEvent = Updates();
            var mapEvent = underlyingEvent.Map(map);
            var currentValue = Sample();
            var mappedValue = map(currentValue);
            var behavior = mapEvent.ToBehavior(mappedValue);
            behavior.RegisterFinalizer(mapEvent);
            return behavior;
        }

        /// <summary>
        /// Sample the behavior's current value.
        /// </summary>
        /// <remarks>
        /// This should generally be avoided in favor of Value().Listen(..) so you don't
        /// miss any updates, but in many circumstances it makes sense.
        ///
        /// It can be best to use it inside an explicit transaction (using Transaction.Run()).
        /// For example, a b.sample() inside an explicit transaction along with a
        /// b.Updates().Listen(..) will capture the current value and any updates without risk
        /// of missing any in between.
        /// </remarks>
        public T Sample()
        {
            // Here's the comment from the Java implementation:
            // 
            //     Since pointers in Java are atomic, we don't need to explicitly create a
            //     transaction.
            //
            // In C# T could be either a reference type or a value type. Question:
            // Can we assume we don't require a transaction here?
            return value;
        }

        /// <summary>
        /// An event that gives the updates for the behavior. If this behavior was created
        /// with a hold, then updates() gives you an event equivalent to the one that was held.
        /// </summary>
        public Event<T> Updates()
        {
            return this.Source;
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
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
        public Behavior<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var coalesceEvent = Updates().Coalesce((a, b) => b);
            var currentValue = Sample();
            var tuple = snapshot(currentValue, initState);
            var loop = new EventLoop<Tuple<TB, TS>>();
            var loopBehavior = loop.ToBehavior(tuple);
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
        internal T NewValue()
        {
            return valueUpdate.HasValue ? valueUpdate.Value() : value;
        }

        /// <summary>
        /// An event that is guaranteed to fire once when you listen to it, giving
        /// the current value of the behavior, and thereafter behaves like updates(),
        /// firing for each update to the behavior's value.
        /// </summary>
        /// <param name="transaction">The transaction to run the Value operation on</param>
        /// <returns>An event that will fire when it's listened to, and every time it's 
        /// value changes thereafter</returns>
        internal Event<T> Value(Transaction transaction)
        {
            var valueEvent = new BehaviorValueEvent<T>(this, transaction);

            // Needed in case of an initial value and an update
            // in the same transaction.
            var result = new LastFiringEvent<T>(valueEvent, transaction);
            result.RegisterFinalizer(valueEvent);
            return result;
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <returns>The IEventListener registered with the underlying event.</returns>
        private IEventListener<T> ListenForEventFirings()
        {
            return this.Run<IEventListener<T>>(this.ListenForEventFirings);
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <param name="transaction">The transaction to schedule the listen on.</param>
        /// <returns>The IEventListener registered with the underlying event.</returns>
        private IEventListener<T> ListenForEventFirings(Transaction transaction)
        {
            var callback = new ActionCallback<T>(ScheduleApplyValueUpdate);
            var result = this.Source.Listen(transaction, callback, Rank.Highest);
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
                transaction.Last(ApplyValueUpdate);
            }

            valueUpdate = new Maybe<T>(update);
        }

        /// <summary>
        /// Store the updated value into the current value, and set the updated value to null.
        /// </summary>
        private void ApplyValueUpdate()
        {
            value = valueUpdate.Value();
            valueUpdate = Maybe<T>.Null;
        }
    }
}