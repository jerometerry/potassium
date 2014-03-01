namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a time varying value
    /// </summary>
    /// <remarks>Behaviors generally change over time, but constant behaviors are ones that choose not to.</remarks>
    /// <typeparam name="TA">The type of values that will be fired through the behavior.</typeparam>
    public class Behavior<TA> : Observable<TA>
    {
        /// <summary>
        /// The current value of the Behavior, updated after the underlying event fires.
        /// </summary>
        private TA value;
        
        /// <summary>
        /// Holding tank for updates from the underlying Event, waiting to be 
        /// moved into the current value of the Behavior.
        /// </summary>
        private Maybe<TA> valueUpdate = Maybe<TA>.Null;

        /// <summary>
        /// Listener that listens for firings from the underlying Event.
        /// </summary>
        private IEventListener<TA> valueUpdateListener;

        /// <summary>
        /// A behavior with a time varying value
        /// </summary>
        /// <param name="initValue"></param>
        public Behavior(TA initValue)
            : this(new Event<TA>(), initValue)
        {
            this.RegisterFinalizer(this.Source);
        }

        /// <summary>
        /// A behavior with a time varying value
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="initValue"></param>
        public Behavior(Event<TA> evt, TA initValue)
        {
            this.Source = evt;
            this.value = initValue;
            this.valueUpdateListener = ListenForEventFirings();
        }

        /// <summary>
        /// The underlying Event that the Value of the current Behavior is updated with.
        /// </summary>
        protected Event<TA> Source { get; private set; }

        /// <summary>
        /// A behavior with a constant value.
        /// </summary>
        public static Behavior<TA> Constant(TA initValue)
        {
            var behavior = new Behavior<TA>(new StaticEvent<TA>(), initValue);
            behavior.RegisterFinalizer(behavior.Source);
            return behavior;
        }

        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        public static Behavior<TB> Apply<TB>(Behavior<Func<TA, TB>> bf, Behavior<TA> source)
        {
            var evt = new BehaviorApplyEvent<TA, TB>(bf, source);
            var behavior = evt.Behavior;
            behavior.RegisterFinalizer(evt);
            return behavior;
        }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        public static Behavior<TA> SwitchB(Behavior<Behavior<TA>> source)
        {
            var innerBehavior = source.Sample();
            var initValue = innerBehavior.Sample();
            var sink = new SwitchBehaviorEvent<TA>(source);
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
        /// SwitchE operation that takes a thread. This ensures that any other
        /// actions triggered during SwitchE requiring a transaction all get the same instance.</remarks>
        public static Event<TA> SwitchE(Behavior<Event<TA>> behavior)
        {
            return TransactionContext.Current.Run(t => SwitchE(t, behavior));
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        public static Behavior<TC> Lift<TB, TC>(Func<TA, TB, TC> lift, Behavior<TA> a, Behavior<TB> b)
        {
            return a.Lift(lift, b);
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        public static Behavior<TD> Lift<TB, TC, TD>(Func<TA, TB, TC, TD> f, Behavior<TA> a, Behavior<TB> b, Behavior<TC> c)
        {
            return a.Lift(f, b, c);
        }

        /// <summary>
        /// Dispose of the current Behavior
        /// </summary>
        public override void Dispose()
        {
            if (this.valueUpdateListener != null)
            {
                this.valueUpdateListener = null;
            }

            if (this.Source != null)
            {
                this.Source = null;
            }

            base.Dispose();
        }

        /// <summary>
        /// Fire the given value to all registered listeners 
        /// </summary>
        /// <param name="a">The value to be fired</param>
        public override void Fire(TA a)
        {
            this.Source.Fire(a);
        }

        /// <summary>
        /// Listen to the current Behavior for updates, which fires immediate with the 
        /// Behaviors current value, and every time the underlying event fires.
        /// </summary>
        /// <param name="action">The action to take when the Behavior's underlying event fires</param>
        /// <returns>The listener</returns>
        public override IEventListener<TA> Listen(Action<TA> action)
        {
            var v = this.Value();
            var l = v.Listen(action) as EventListener<TA>;
            l.RegisterFinalizer(v);
            return l;
        }

        /// <summary>
        /// Listen to the current Behavior for updates, but don't fire the initial value
        /// </summary>
        /// <param name="action">The action to take when the Behavior's underlying event fires</param>
        /// <returns>The listener</returns>
        public override IEventListener<TA> ListenSuppressed(Action<TA> action)
        {
            var v = this.Updates();
            var l = v.Listen(action) as EventListener<TA>;
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
        public Event<TA> Value()
        {
            return TransactionContext.Current.Run(Value);
        }

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        public Behavior<TB> Map<TB>(Func<TA, TB> map)
        {
            var underlyingEvent = Updates();
            var mapEvent = underlyingEvent.Map(map);
            var currentValue = Sample();
            var mappedValue = map(currentValue);
            var behavior = mapEvent.Hold(mappedValue);
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
        public TA Sample()
        {
            // Here's the comment from the Java implementation:
            // 
            //     Since pointers in Java are atomic, we don't need to explicitly create a
            //     transaction.
            //
            // In C# TA could be either a reference type or a value type. Question:
            // Can we assume we don't require a transaction here?
            return value;
        }

        /// <summary>
        /// An event that gives the updates for the behavior. If this behavior was created
        /// with a hold, then updates() gives you an event equivalent to the one that was held.
        /// </summary>
        public Event<TA> Updates()
        {
            return this.Source;
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        public Behavior<TC> Lift<TB, TC>(Func<TA, TB, TC> lift, Behavior<TB> behavior)
        {
            Func<TA, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            var bf = Map(ffa);
            var result = Behavior<TB>.Apply(bf, behavior);
            result.RegisterFinalizer(bf);
            return result;
        }

        /// <summary>
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        public Behavior<TB> Collect<TB, TS>(TS initState, Func<TA, TS, Tuple<TB, TS>> snapshot)
        {
            var coalesceEvent = Updates().Coalesce((a, b) => b);
            var currentValue = Sample();
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
        public Behavior<TD> Lift<TB, TC, TD>(Func<TA, TB, TC, TD> lift, Behavior<TB> b, Behavior<TC> c)
        {
            Func<TA, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            var bf = Map(map);
            var l1 = Behavior<TB>.Apply(bf, b);

            var result = Behavior<TC>.Apply(l1, c);
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
        internal TA NewValue()
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
        internal Event<TA> Value(Transaction transaction)
        {
            var valueEvent = new BehaviorValueEvent<TA>(this, transaction);

            // Needed in case of an initial value and an update
            // in the same transaction.
            var result = valueEvent.LastFiringOnly(transaction);
            result.RegisterFinalizer(valueEvent);
            return result;
        }

        /// <summary>
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="behavior">The behavior that wraps the event</param>
        /// <returns>The unwrapped event</returns>
        internal static Event<TA> SwitchE(Transaction transaction, Behavior<Event<TA>> behavior)
        {
            return new SwitchEvent<TA>(transaction, behavior);
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <returns>The IEventListener registered with the underlying event.</returns>
        private IEventListener<TA> ListenForEventFirings()
        {
            return TransactionContext.Current.Run(t => this.ListenForEventFirings(t));
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <param name="transaction">The transaction to schedule the listen on.</param>
        /// <returns>The IEventListener registered with the underlying event.</returns>
        private IEventListener<TA> ListenForEventFirings(Transaction transaction)
        {
            var action = new SodiumAction<TA>(ScheduleApplyValueUpdate);
            var result = this.Source.Listen(transaction, action, Rank.Highest);
            return result;
        }

        /// <summary>
        /// Store the updated value, and schedule a Transaction.Last action that will move the updated value 
        /// into the current value.
        /// </summary>
        /// <param name="transaction">The transaction to schedule the value update on</param>
        /// <param name="update">The updated value</param>
        private void ScheduleApplyValueUpdate(Transaction transaction, TA update)
        {
            if (!valueUpdate.HasValue)
            {
                transaction.Last(ApplyValueUpdate);
            }

            valueUpdate = new Maybe<TA>(update);
        }

        /// <summary>
        /// Store the updated value into the current value, and set the updated value to null.
        /// </summary>
        private void ApplyValueUpdate()
        {
            value = valueUpdate.Value();
            valueUpdate = Maybe<TA>.Null;
        }
    }
}