namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a time varying value. It starts with an initial value which 
    /// gets updated as the underlying Event is fired.
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the Behavior.</typeparam>
    public class Behavior<T> : SodiumObject
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
        /// It can be best to use it inside an explicit scheduler (using ActionSchedulerContext.Current.Run()).
        /// For example, a b.Value inside an explicit scheduler along with a
        /// b.Source.Listen(..) will capture the current value and any updates without risk
        /// of missing any in between.
        /// </remarks>
        public T Value { get; private set; }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        public static Behavior<T> Unwrap(Behavior<Behavior<T>> source)
        {
            var innerBehavior = source.Value;
            var initValue = innerBehavior.Value;
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
        /// <remarks>ActionSchedulerContext.Current.Run is used to invoke the overload of the 
        /// UnwrapEvent operation that takes a thread. This ensures that any other
        /// actions triggered during UnwrapEvent requiring a scheduler all get the same instance.</remarks>
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
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the underlying event fires</param>
        /// <returns>The event listener</returns>
        public IEventListener<T> Listen(Action<T> callback)
        {
            return this.Source.Listen(callback);
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the underlying event fires</param>
        /// <param name="rank">The rank of the action, used as a superior to the rank of the underlying action.</param>
        /// <returns>The event listener</returns>
        public IEventListener<T> Listen(ISodiumCallback<T> callback, Rank rank)
        {
            return this.Source.Listen(callback, rank);
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the underlying event fires</param>
        /// <param name="rank">The rank of the action, used as a superior to the rank of the underlying action.</param>
        /// <param name="scheduler">The scheduler used to order actions</param>
        /// <returns>The event listener</returns>
        public IEventListener<T> Listen(ISodiumCallback<T> callback, Rank rank, Scheduler scheduler)
        {
            return this.Source.Listen(callback, rank, scheduler);
        }

        /// <summary>
        /// An event that is guaranteed to fire once when you listen to it, giving
        /// the current value of the behavior, and thereafter behaves like updates(),
        /// firing for each update to the behavior's value.
        /// </summary>
        /// <returns>An event that will fire when it's listened to, and every time it's 
        /// value changes thereafter</returns>
        /// <remarks>ActionSchedulerContext.Current.Run is used to invoke the overload of the 
        /// Value operation that takes a thread. This ensures that any other
        /// actions triggered during Value requiring a scheduler all get the same instance.</remarks>
        public Event<T> Values()
        {
            return this.RunScheduler<Event<T>>(this.Values);
        }

        /// <summary>
        /// An event that is guaranteed to fire once when you listen to it, giving
        /// the current value of the behavior, and thereafter behaves like updates(),
        /// firing for each update to the behavior's value.
        /// </summary>
        /// <param name="scheduler">The scheduler to run the Value operation on</param>
        /// <returns>An event that will fire when it's listened to, and every time it's 
        /// value changes thereafter</returns>
        public Event<T> Values(Scheduler scheduler)
        {
            var valueEvent = new BehaviorValueEvent<T>(this, scheduler);

            // Needed in case of an initial value and an update
            // in the same scheduler.
            var result = new LastFiringEvent<T>(valueEvent, scheduler);
            result.RegisterFinalizer(valueEvent);
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
        public T GetNewValue()
        {
            return valueUpdate.HasValue ? valueUpdate.Value() : Value;
        }

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        public Behavior<TB> Map<TB>(Func<T, TB> map)
        {
            var underlyingEvent = Source;
            var mapEvent = underlyingEvent.Map(map);
            var currentValue = Value;
            var mappedValue = map(currentValue);
            var behavior = mapEvent.ToBehavior(mappedValue);
            behavior.RegisterFinalizer(mapEvent);
            return behavior;
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
            var coalesceEvent = Source.Coalesce((a, b) => b);
            var currentValue = Value;
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
            return this.RunScheduler<IEventListener<T>>(this.ListenForEventFirings);
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <param name="scheduler">The scheduler to schedule the listen on.</param>
        /// <returns>The IEventListener registered with the underlying event.</returns>
        private IEventListener<T> ListenForEventFirings(Scheduler scheduler)
        {
            var callback = new ActionCallback<T>(ScheduleApplyValueUpdate);
            var result = this.Listen(callback, Rank.Highest, scheduler);
            return result;
        }

        /// <summary>
        /// Store the updated value, and schedule a Scheduler.Last action that will move the updated value 
        /// into the current value.
        /// </summary>
        /// <param name="scheduler">The scheduler to schedule the value update on</param>
        /// <param name="update">The updated value</param>
        private void ScheduleApplyValueUpdate(T update, Scheduler scheduler)
        {
            if (!valueUpdate.HasValue)
            {
                scheduler.Medium(ApplyValueUpdate);
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