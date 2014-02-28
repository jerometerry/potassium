namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a time varying value
    /// </summary>
    /// <remarks>Behaviors generally change over time, but constant behaviors are ones that choose not to.</remarks>
    /// <typeparam name="TA">The type of values that will be fired through the behavior.</typeparam>
    public class Behavior<TA> : SodiumItem
    {
        private TA value;
        
        /// <summary>
        /// Holding tank for updates from the underlying Event, waiting to be 
        /// moved into the current value of the Behavior.
        /// </summary>
        private Maybe<TA> valueUpdate = Maybe<TA>.Null;

        private IEventListener<TA> eventListener;

        /// <summary>
        /// A behavior with a time varying value
        /// </summary>
        /// <param name="initValue"></param>
        public Behavior(TA initValue)
            : this(initValue, false)
        {
        }

        public Behavior(TA initValue, bool allowAutoDispose)
            : this(new Event<TA>(true), initValue, allowAutoDispose)
        {
        }

        /// <summary>
        /// A behavior with a time varying value
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="initValue"></param>
        public Behavior(Event<TA> evt, TA initValue, bool allowAutoDispose)
            : base(allowAutoDispose)
        {
            this.Event = evt;
            this.value = initValue;
            this.eventListener = ListenForEventFirings(true);
            Metrics.BehaviorAllocations++;
        }

        protected Event<TA> Event { get; private set; }

        /// <summary>
        /// A behavior with a constant value.
        /// </summary>
        public static Behavior<TA> Constant(TA initValue)
        {
            return Constant(initValue, true);
        }

        public static Behavior<TA> Constant(TA initValue, bool allowAutoDispose)
        {
            return new Behavior<TA>(new StaticEvent<TA>(true), initValue, allowAutoDispose);
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        public static Behavior<TC> Lift<TB, TC>(Func<TA, TB, TC> lift, Behavior<TA> a, Behavior<TB> b)
        {
            return Lift(lift, a, b, true);
        }

        public static Behavior<TC> Lift<TB, TC>(Func<TA, TB, TC> lift, Behavior<TA> a, Behavior<TB> b, bool allowAutoDispose)
        {
            return a.Lift(lift, b, allowAutoDispose);
        }

        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        public static Behavior<TB> Apply<TB>(Behavior<Func<TA, TB>> bf, Behavior<TA> ba)
        {
            return Apply(bf, ba, true);
        }

        public static Behavior<TB> Apply<TB>(Behavior<Func<TA, TB>> bf, Behavior<TA> ba, bool allowAutoDispose)
        {
            var sink = new BehaviorApplyEvent<TA, TB>(bf, ba, true, allowAutoDispose);
            return sink.Behavior;
        }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        public static Behavior<TA> SwitchB(Behavior<Behavior<TA>> bba)
        {
            return SwitchB(bba, true);
        }

        public static Behavior<TA> SwitchB(Behavior<Behavior<TA>> bba, bool allowAutoDispose)
        {
            var innerBehavior = bba.Sample();
            var initValue = innerBehavior.Sample();
            var sink = new SwitchBehaviorEvent<TA>(bba, true);
            return sink.Hold(initValue, allowAutoDispose);
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
            return SwitchE(behavior, true);
        }

        public static Event<TA> SwitchE(Behavior<Event<TA>> behavior, bool allowAutoDispose)
        {
            return TransactionContext.Current.Run(t => SwitchE(t, behavior, allowAutoDispose));
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        public static Behavior<TD> Lift<TB, TC, TD>(Func<TA, TB, TC, TD> f, Behavior<TA> a, Behavior<TB> b, Behavior<TC> c)
        {
            return Lift(f, a, b, c, true);
        }

        public static Behavior<TD> Lift<TB, TC, TD>(Func<TA, TB, TC, TD> f, Behavior<TA> a, Behavior<TB> b, Behavior<TC> c, bool allowAutoDispose)
        {
            return a.Lift(f, b, c, allowAutoDispose);
        }

        /// <summary>
        /// Fire the given value to all registered listeners 
        /// </summary>
        /// <param name="a">The value to be fired</param>
        public void Fire(TA a)
        {
            AssertNotDisposed();
            this.Event.Fire(a);
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
            AssertNotDisposed();

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
            AssertNotDisposed();
            return this.Event;
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
            return this.Value(true);
        }
        
        public Event<TA> Value(bool allowAutoDispose)
        {
            AssertNotDisposed();
            return TransactionContext.Current.Run(t => Value(t, allowAutoDispose));
        }

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        public Behavior<TB> Map<TB>(Func<TA, TB> map)
        {
            return Map(map, true);
        }

        public Behavior<TB> Map<TB>(Func<TA, TB> map, bool allowAutoDispose)
        {
            AssertNotDisposed();
            
            // No allocations. Just returns the underlying event
            var underlyingEvent = Updates();

            // Creates a new MapEvent
            var mapEvent = underlyingEvent.Map(map, true);

            // No allocations. Just returns the current value
            var currentValue = Sample();
            
            // Mapping function may or may not allocate Events / Behaviors
            // out of our control
            var mappedValue = map(currentValue);
            
            // Creates a new Behavior and a new Event
            var behavior = mapEvent.Hold(mappedValue, allowAutoDispose);
            
            return behavior;
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        public Behavior<TC> Lift<TB, TC>(Func<TA, TB, TC> lift, Behavior<TB> behavior)
        {
            return Lift(lift, behavior, true);
        }

        public Behavior<TC> Lift<TB, TC>(Func<TA, TB, TC> lift, Behavior<TB> behavior, bool allowAutoDispose)
        {
            AssertNotDisposed();
            Func<TA, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            var bf = Map(ffa, true);
            return Behavior<TB>.Apply(bf, behavior, allowAutoDispose);
        }

        /// <summary>
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        public Behavior<TB> Collect<TB, TS>(TS initState, Func<TA, TS, Tuple<TB, TS>> snapshot)
        {
            return Collect(initState, snapshot, true);
        }

        public Behavior<TB> Collect<TB, TS>(TS initState, Func<TA, TS, Tuple<TB, TS>> snapshot, bool allowAutoDispose)
        {
            AssertNotDisposed();
            var coalesceEvent = Updates().Coalesce((a, b) => b, true);
            var currentValue = Sample();
            var tuple = snapshot(currentValue, initState);
            var loop = new EventLoop<Tuple<TB, TS>>(true);
            var loopBehavior = loop.Hold(tuple, true);
            var snapshotBehavior = loopBehavior.Map(x => x.Item2, true);
            var coalesceSnapshotEvent = coalesceEvent.Snapshot(snapshotBehavior, snapshot, true);
            loop.Loop(coalesceSnapshotEvent);
            return loopBehavior.Map(x => x.Item1, allowAutoDispose);
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        public Behavior<TD> Lift<TB, TC, TD>(Func<TA, TB, TC, TD> lift, Behavior<TB> b, Behavior<TC> c, bool allowAutoDispose)
        {
            AssertNotDisposed();
            Func<TA, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            var bf = Map(map, true);
            var l1 = Behavior<TB>.Apply(bf, b, true);
            return Behavior<TC>.Apply(l1, c, allowAutoDispose);
        }

        /// <summary>
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="behavior">The behavior that wraps the event</param>
        /// <returns>The unwrapped event</returns>
        internal static Event<TA> SwitchE(Transaction transaction, Behavior<Event<TA>> behavior, bool allowAutoDispose)
        {
            return new SwitchEvent<TA>(transaction, behavior, allowAutoDispose);
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
        internal Event<TA> Value(Transaction transaction, bool allowAutoDispose)
        {
            var valueEvent = new BehaviorValueEvent<TA>(this, this.Event, transaction, true);

            // Needed in case of an initial value and an update
            // in the same transaction.
            return valueEvent.LastFiringOnly(transaction, allowAutoDispose);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Metrics.BehaviorDeallocations++;

                if (this.eventListener != null)
                {
                    this.eventListener.AutoDispose();
                    this.eventListener = null;
                }

                if (this.Event != null)
                {
                    this.Event.AutoDispose();
                    this.Event = null;
                }
            }
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <returns>The IEventListener registered with the underlying event.</returns>
        private IEventListener<TA> ListenForEventFirings(bool allowAutoDispose)
        {
            return TransactionContext.Current.Run(t => this.ListenForEventFirings(t, allowAutoDispose));
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <param name="transaction">The transaction to schedule the listen on.</param>
        /// <returns>The IEventListener registered with the underlying event.</returns>
        private IEventListener<TA> ListenForEventFirings(Transaction transaction, bool allowAutoDispose)
        {
            var action = new SodiumAction<TA>(ScheduleApplyValueUpdate);
            return this.Event.Listen(transaction, action, Rank.Highest, allowAutoDispose);
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