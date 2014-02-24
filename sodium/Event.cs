namespace Sodium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Event<TA>
    {
        private readonly List<ITrigger<TA>> triggers = new List<ITrigger<TA>>();
        private readonly List<TA> firings = new List<TA>();
        private readonly List<IListener> listeners = new List<IListener>();
        private readonly Node node = new Node();

        internal Event()
        {
        }

        ~Event()
        {
            Close();
        }

        internal Node Node
        {
            get { return node; }
        }

        /// <summary>
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        ///
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        /// </summary>
        public static Event<TA> MergeWith(Func<TA, TA, TA> f, Event<TA> event1, Event<TA> event2)
        {
            return Merge(event1, event2).Coalesce(f);
        }

        /// <summary>
        /// Merge two streams of events of the same type.
        ///
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event firings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </summary>
        public static Event<TA> Merge(Event<TA> event1, Event<TA> event2)
        {
            var sink = new MergeEventSink<TA>(event1, event2);
            var trigger = new Trigger<TA>(sink.Send);
            var l1 = event1.Listen(sink.Node, trigger);
            var l2 = event2.Listen(sink.Node, trigger);
            return sink.RegisterListener(l1).RegisterListener(l2);
        }

        /// <summary>
        /// Listen for firings of this event. The returned Listener has an unlisten()
        /// method to cause the listener to be removed. This is the observer pattern.
        /// </summary>
        public IListener Listen(Action<TA> trigger)
        {
            return Listen(Node.Null, new Trigger<TA>((t, a) => trigger(a)));
        }

        /// <summary>
        /// Transform the event's value according to the supplied function.
        /// </summary>
        public Event<TB> Map<TB>(Func<TA, TB> map)
        {
            var sink = new MapEventSink<TA, TB>(this, map);
            var l = Listen(sink.Node, new Trigger<TA>(sink.MapAndSend));
            return sink.RegisterListener(l);
        }

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        /// </summary>
        public Behavior<TA> Hold(TA initValue)
        {
            return Transaction.Apply(t => new Behavior<TA>(LastFiringOnly(t), initValue));
        }

        /// <summary>
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        /// </summary>
        public Event<TB> Snapshot<TB>(Behavior<TB> behavior)
        {
            return Snapshot(behavior, (a, b) => b);
        }

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        public Event<TC> Snapshot<TB, TC>(Behavior<TB> behavior, Func<TA, TB, TC> snapshot)
        {
            var sink = new SnapshotEventSink<TA, TB, TC>(this, snapshot, behavior);
            var trigger = new Trigger<TA>(sink.SnapshotAndSend);
            var listener = Listen(sink.Node, trigger);
            return sink.RegisterListener(listener);
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        public Event<TA> Delay()
        {
            var sink = new EventSink<TA>();
            var trigger = new Trigger<TA>((t, a) => t.Post(() => sink.Send(a)));
            var listener = Listen(sink.Node, trigger);
            return sink.RegisterListener(listener);
        }

        /// <summary>
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        ///
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </summary>
        public Event<TA> Coalesce(Func<TA, TA, TA> coalesce)
        {
            return Transaction.Apply(t => Coalesce(t, coalesce));
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        public Event<TA> Filter(Func<TA, bool> predicate)
        {
            var sink = new FilterEventSink<TA>(this, predicate);
            var handler = new Trigger<TA>(sink.SendIfNotFiltered);
            var l = Listen(sink.Node, handler);
            return sink.RegisterListener(l);
        }

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        public Event<TA> FilterNotNull()
        {
            return Filter(a => a != null);
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        public Event<TA> Gate(Behavior<bool> predicate)
        {
            Func<TA, bool, Maybe<TA>> snapshot = (a, p) => p ? new Maybe<TA>(a) : null;
            return Snapshot(predicate, snapshot).FilterNotNull().Map(a => a.Value());
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        public Event<TB> Collect<TB, TS>(TS initState, Func<TA, TS, Tuple2<TB, TS>> snapshot)
        {
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
            var ebs = Snapshot(s, snapshot);
            var eb = ebs.Map(bs => bs.V1);
            var evt = ebs.Map(bs => bs.V2);
            es.Loop(evt);
            return eb;
        }

        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        public Behavior<TS> Accum<TS>(TS initState, Func<TA, TS, TS> snapshot)
        {
            var loop = new EventLoop<TS>();
            var behavior = loop.Hold(initState);
            var snapshotEvent = Snapshot(behavior, snapshot);
            loop.Loop(snapshotEvent);
            return snapshotEvent.Hold(initState);
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        public Event<TA> Once()
        {
            // This is a bit long-winded but it's efficient because it deregisters
            // the listener.
            var la = new IListener[1];
            var sink = new OnceEventSink<TA>(this, la);
            la[0] = this.Listen(sink.Node, new Trigger<TA>((t, a) => sink.Send(la, t, a)));
            return sink.RegisterListener(la[0]);
        }

        public void Close()
        {
            foreach (var l in this.listeners)
            {
                l.Unlisten();
            }

            listeners.Clear();
        }

        internal void Unlisten(ITrigger<TA> trigger, Node target)
        {
            lock (Transaction.ListenersLock)
            {
                this.RemoveTrigger(trigger);
                this.Node.UnlinkTo(target);
            }
        }

        internal Event<TA> RegisterListener(IListener listener)
        {
            listeners.Add(listener);
            return this;
        }

        internal void RemoveTrigger(ITrigger<TA> trigger)
        {
            this.triggers.Remove(trigger);
        }

        internal virtual void Send(Transaction transaction, TA firing)
        {
            this.ScheduleClearFirings(transaction);
            this.RecordFiring(firing);
            this.Fire(transaction, firing);
        }

        /// <summary>
        /// Clean up the output by discarding any firing other than the last one. 
        /// </summary>
        internal Event<TA> LastFiringOnly(Transaction transaction)
        {
            return Coalesce(transaction, (a, b) => b);
        }

        internal IListener Listen(Node target, ITrigger<TA> trigger)
        {
            return Transaction.Apply(t => Listen(target, t, trigger, false));
        }

        internal IListener Listen(Node target, Transaction transaction, ITrigger<TA> trigger, bool suppressEarlierFirings)
        {
            lock (Transaction.ListenersLock)
            {
                transaction.LinkNodes(this.node, target);
                this.triggers.Add(trigger);
            }

            var events = SampleNow();
            if (events != null)
            {
                // In cases like Value(), we start with an initial value.
                transaction.Fire(trigger, events);
            }

            if (!suppressEarlierFirings)
            {
                // Anything sent already in this transaction must be sent now so that
                // there's no order dependency between send and listen.
                transaction.Fire(trigger, firings);
            }

            return new Listener<TA>(this, trigger, target);
        }

        protected internal virtual TA[] SampleNow()
        {
            return null;
        }

        private Event<TA> Coalesce(Transaction transaction, Func<TA, TA, TA> coalesce)
        {
            var sink = new CoalesceEventSink<TA>(this, coalesce);
            var trigger = new CoalesceTrigger<TA>(sink, coalesce);
            var listener = Listen(sink.Node, transaction, trigger, false);
            return sink.RegisterListener(listener);
        }

        private void ScheduleClearFirings(Transaction transaction)
        {
            var noFirings = !this.firings.Any();
            if (noFirings)
            {
                // clear any added firings during Transaction.CloseLastActions
                transaction.Last(() => this.firings.Clear());
            }
        }

        private void RecordFiring(TA firing)
        {
            firings.Add(firing);
        }

        private void Fire(Transaction transaction, TA firing)
        {
            var clone = new List<ITrigger<TA>>(this.triggers);
            foreach (var trigger in clone)
            {
                trigger.Fire(transaction, firing);
            }
        }
    }
}