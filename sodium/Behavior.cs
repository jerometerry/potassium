namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a time varying value
    /// </summary>
    /// <remarks>Behaviors generally change over time, but constant behaviors are ones that choose not to.</remarks>
    /// <typeparam name="TA"></typeparam>
    public class Behavior<TA>
    {
        private TA value;
        private Maybe<TA> valueUpdate = Maybe<TA>.Null;
        private IListener listener;
        private Event<TA> evt;

        /// <summary>
        /// A behavior with a time varying value
        /// </summary>
        /// <param name="initValue"></param>
        public Behavior(TA initValue)
            : this(new Event<TA>(), initValue)
        {
        }

        /// <summary>
        /// A behavior with a time varying value
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="initValue"></param>
        internal Behavior(Event<TA> evt, TA initValue)
        {
            this.evt = evt;
            this.value = initValue;
            ListenForEventFirings();
        }

        /// <summary>
        /// A behavior with a constant value.
        /// </summary>
        public static Behavior<TA> Constant(TA initValue)
        {
            return new Behavior<TA>(new StaticEvent<TA>(), initValue);
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        public static Behavior<TC> Lift<TB, TC>(Func<TA, TB, TC> lift, Behavior<TA> a, Behavior<TB> b)
        {
            return a.Lift(lift, b);
        }

        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        public static Behavior<TB> Apply<TB>(Behavior<Func<TA, TB>> bf, Behavior<TA> ba)
        {
            var sink = new BehaviorApplyEvent<TA,TB>(bf,ba);
            return sink.Behavior;
        }

        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        public static Behavior<TA> SwitchB(Behavior<Behavior<TA>> bba)
        {
            var innerBehavior = bba.Sample();
            var initValue = innerBehavior.Sample();
            var sink = new SwitchBehaviorEvent<TA>(bba);
            return sink.Hold(initValue);
        }

        /// <summary>
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        /// </summary>
        public static Event<TA> SwitchE(Behavior<Event<TA>> bea)
        {
            return TransactionContext.Run(t => SwitchE(t, bea));
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        public static Behavior<TD> Lift<TB, TC, TD>(Func<TA, TB, TC, TD> f, Behavior<TA> a, Behavior<TB> b, Behavior<TC> c)
        {
            return a.Lift(f, b, c);
        }

        public void Loop(Behavior<TA> b)
        {
            evt.Loop(b.Updates());
            var v = b.Sample();
            SetValue(v);
        }

        public void Fire(TA a)
        {
            evt.Fire(a);
        }

        /// <summary>
        /// Stop listening for event occurrences
        /// </summary>
        public void Stop()
        {
            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }

            if (evt != null)
            {
                evt.Stop();
                evt = null;
            }
        }

        /// <summary>
        /// Sample the behavior's current value.
        /// </summary>
        /// <remarks>
        /// This should generally be avoided in favour of Value().Listen(..) so you don't
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
            return evt;
        }

        /// <summary>
        /// An event that is guaranteed to fire once when you listen to it, giving
        /// the current value of the behavior, and thereafter behaves like updates(),
        /// firing for each update to the behavior's value.
        /// </summary>
        public Event<TA> Value()
        {
            return TransactionContext.Run(Value);
        }

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        public Behavior<TB> Map<TB>(Func<TA, TB> map)
        {
            return Updates().Map(map).Hold(map(Sample()));
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        public Behavior<TC> Lift<TB, TC>(Func<TA, TB, TC> lift, Behavior<TB> behavior)
        {
            Func<TA, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            var bf = Map(ffa);
            return Behavior<TB>.Apply(bf, behavior);
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
            var loop = new Event<Tuple<TB, TS>>();
            var loopBehavior = loop.Hold(tuple);
            var snapshotBehavior = loopBehavior.Map(x => x.Item2);
            var coalesceSnapshotEvent = coalesceEvent.Snapshot(snapshotBehavior, snapshot);
            loop.Loop(coalesceSnapshotEvent);
            return loopBehavior.Map(x => x.Item1);
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        public Behavior<TD> Lift<TB, TC, TD>(Func<TA, TB, TC, TD> lift, Behavior<TB> b, Behavior<TC> c)
        {
            Func<TA, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            var bf = Map(map);
            var l1 = Behavior<TB>.Apply(bf, b);
            return Behavior<TC>.Apply(l1, c);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// The value including any updates that have happened in this transaction.
        /// </returns>
        internal TA NewValue()
        {
            return valueUpdate.HasValue ? valueUpdate.Value() : value;
        }

        internal Event<TA> Value(Transaction transaction)
        {
            var sink = new BehaviorValueEvent<TA>(this, evt, transaction);
            // Needed in case of an initial value and an update
            // in the same transaction.
            return sink.LastFiringOnly(transaction);
        }

        protected void SetValue(TA v)
        {
            value = v;
        }

        private static Event<TA> SwitchE(Transaction transaction, Behavior<Event<TA>> behavior)
        {
            return new SwitchEvent<TA>(transaction, behavior);
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        private void ListenForEventFirings()
        {
            TransactionContext.Run(t =>
            {
                this.ListenForEventFirings(t);
                return Unit.Value;
            });
        }

        /// <summary>
        /// Listen to the underlying event for firings
        /// </summary>
        /// <param name="transaction"></param>
        private void ListenForEventFirings(Transaction transaction)
        {
            var callback = new Callback<TA>(ScheduleApplyValueUpdate);
            listener = evt.Listen(transaction, callback, Rank.Highest);
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