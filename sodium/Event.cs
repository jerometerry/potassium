namespace Sodium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Event<TA>
    {
        private readonly List<ICallback<TA>> callbacks = new List<ICallback<TA>>();
        private readonly List<TA> firings = new List<TA>();
        private readonly List<IListener> listeners = new List<IListener>();
        
        /// <summary>
        /// The rank of the current Event. Default to rank zero
        /// </summary>
        private readonly Rank rank = new Rank();

        internal Event()
        {
        }

        ~Event()
        {
            Close();
        }

        internal Rank Rank
        {
            get { return this.rank; }
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
            var callback = new Callback<TA>(sink.Fire);
            var l1 = event1.Listen(callback, sink.Rank);
            var l2 = event2.Listen(callback, sink.Rank);
            return sink.RegisterListener(l1).RegisterListener(l2);
        }

        /// <summary>
        /// Listen for firings of this event. The returned Listener has an unlisten()
        /// method to cause the listener to be removed. This is the observer pattern.
        /// </summary>
        public IListener Listen(Action<TA> callback)
        {
            return Listen(new Callback<TA>((t, a) => callback(a)), Rank.Highest);
        }

        /// <summary>
        /// Transform the event's value according to the supplied function.
        /// </summary>
        public Event<TB> Map<TB>(Func<TA, TB> map)
        {
            var sink = new MapEventSink<TA, TB>(this, map);
            var l = Listen(new Callback<TA>(sink.MapAndSend), sink.Rank);
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
            return Transaction.Run(t => new Behavior<TA>(LastFiringOnly(t), initValue));
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
            var callback = new Callback<TA>(sink.SnapshotAndSend);
            var listener = Listen(callback, sink.Rank);
            return sink.RegisterListener(listener);
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        public Event<TA> Delay()
        {
            var sink = new EventSink<TA>();
            var callback = new Callback<TA>((t, a) => t.Post(() => sink.Send(a)));
            var listener = Listen(callback, sink.Rank);
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
            return Transaction.Run(t => Coalesce(t, coalesce));
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        public Event<TA> Filter(Func<TA, bool> predicate)
        {
            var sink = new FilterEventSink<TA>(this, predicate);
            var callback = new Callback<TA>(sink.FireIfNotFiltered);
            var l = Listen(callback, sink.Rank);
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
            la[0] = Listen(new Callback<TA>((t, a) => sink.Fire(la, t, a)), sink.Rank);
            return sink.RegisterListener(la[0]);
        }

        public void Close()
        {
            foreach (var l in listeners)
            {
                l.Unlisten();
            }

            listeners.Clear();
        }

        internal static void InvokeCallbacks(Transaction transaction, ICallback<TA> callback, IEnumerable<TA> payloads)
        {
            foreach (var payload in payloads)
            {
                callback.Invoke(transaction, payload);
            }
        }

        internal void Unlisten(ICallback<TA> callback, Rank superior)
        {
            lock (Constants.ListenersLock)
            {
                RemoveCallback(callback);
                this.Rank.RemoveSuperior(superior);
            }
        }

        internal Event<TA> RegisterListener(IListener listener)
        {
            listeners.Add(listener);
            return this;
        }

        internal void RemoveCallback(ICallback<TA> callback)
        {
            callbacks.Remove(callback);
        }

        /// <summary>
        /// Fire the given value to all 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="firing"></param>
        internal virtual void Fire(Transaction transaction, TA firing)
        {
            var noFirings = !firings.Any();
            if (noFirings)
            {
                // clear any added firings during Transaction.CloseLastActions
                transaction.Last(() => firings.Clear());
            }
            
            firings.Add(firing);
            
            var clone = new List<ICallback<TA>>(callbacks);
            foreach (var callback in clone)
            {
                callback.Invoke(transaction, firing);
            }
        }

        /// <summary>
        /// Clean up the output by discarding any firing other than the last one. 
        /// </summary>
        internal Event<TA> LastFiringOnly(Transaction transaction)
        {
            return Coalesce(transaction, (a, b) => b);
        }

        internal IListener Listen(ICallback<TA> callback, Rank superior)
        {
            return Transaction.Run(t => ListenUnsuppressed(t, callback, superior));
        }

        internal IListener ListenUnsuppressed(Transaction transaction, ICallback<TA> callback, Rank superior)
        {
            RegisterCallback(transaction, callback, superior);
            InitialFire(transaction, callback);
            Refire(transaction, callback);
            return new Listener<TA>(this, callback, superior);
        }

        internal IListener ListenSuppressed(Transaction transaction, ICallback<TA> callback, Rank superior)
        {
            RegisterCallback(transaction, callback, superior);
            InitialFire(transaction, callback);
            return new Listener<TA>(this, callback, superior);
        }

        protected internal virtual TA[] InitialFirings()
        {
            return null;
        }

        private void RegisterCallback(Transaction transaction, ICallback<TA> callback, Rank superior)
        {
            lock (Constants.ListenersLock)
            {
                if (this.rank.AddSuperior(superior))
                {
                    transaction.NodeRanksModified = true;
                }

                callbacks.Add(callback);
            }
        }

        private void InitialFire(Transaction transaction, ICallback<TA> callback)
        {
            var payloads = InitialFirings();
            if (payloads != null)
            {
                InvokeCallbacks(transaction, callback, payloads);
            }
        }

        private Event<TA> Coalesce(Transaction transaction, Func<TA, TA, TA> coalesce)
        {
            var sink = new CoalesceEventSink<TA>(this, coalesce);
            var callback = new CoalesceCallback<TA>(sink, coalesce);
            var listener = ListenUnsuppressed(transaction, callback, sink.Rank);
            return sink.RegisterListener(listener);
        }

        /// <summary>
        /// Anything fired already in this transaction must be refired now so that
        /// there's no order dependency between send and listen.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="callback"></param>
        private void Refire(Transaction transaction, ICallback<TA> callback)
        {
            InvokeCallbacks(transaction, callback, firings);
        }
    }
}