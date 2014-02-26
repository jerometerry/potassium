namespace Sodium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An Event is a stream of discrete event occurrences
    /// </summary>
    /// <typeparam name="TA"></typeparam>
    public class Event<TA>
    {
        private readonly List<ICallback<TA>> callbacks = new List<ICallback<TA>>();
        private readonly List<TA> firings = new List<TA>();
        private readonly List<IListener> listeners = new List<IListener>();
        
        /// <summary>
        /// The rank of the current Event. Default to rank zero
        /// </summary>
        private readonly Rank rank = new Rank();

        private Event<TA> loopedEvent;

        ~Event()
        {
            this.Stop();
        }

        internal Rank Rank
        {
            get { return this.rank; }
        }

        /// <summary>
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        /// </summary>
        /// <remarks>
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        /// </remarks>
        public static Event<TA> MergeWith(Func<TA, TA, TA> f, Event<TA> event1, Event<TA> event2)
        {
            return Merge(event1, event2).Coalesce(f);
        }

        /// <summary>
        /// Merge two streams of events of the same type.
        /// </summary>
        /// <remarks>
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event firings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </remarks>
        public static Event<TA> Merge(Event<TA> event1, Event<TA> event2)
        {
            return new MergeEvent<TA>(event1, event2);
        }

        public void Loop(Event<TA> evt)
        {
            if (loopedEvent != null)
            {
                throw new ApplicationException("EventLoop looped more than once");
            }

            loopedEvent = evt;
            var listener = evt.Listen(new Callback<TA>(Fire), this.Rank);
            RegisterListener(listener);
        }

        /// <summary>
        /// Fire the given value to all registered listeners 
        /// </summary>
        /// <param name="a"></param>
        public void Fire(TA a)
        {
            TransactionContext.Run(t => { Fire(t, a); return Unit.Value; });
        }

        /// <summary>
        /// Listen for firings of this event. The returned Listener has an Unlisten()
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
            return new MapEvent<TA, TB>(this, map);
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
            return TransactionContext.Run(t => new Behavior<TA>(LastFiringOnly(t), initValue));
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
            return new SnapshotEvent<TA, TB, TC>(this, snapshot, behavior);
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        public Event<TA> Delay()
        {
            return new DelayEvent<TA>(this);
        }

        /// <summary>
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <remarks>
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        public Event<TA> Coalesce(Func<TA, TA, TA> coalesce)
        {
            return TransactionContext.Run(t => Coalesce(t, coalesce));
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        public Event<TA> Filter(Func<TA, bool> predicate)
        {
            return new FilterEvent<TA>(this, predicate);
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
            return this.Snapshot(predicate, snapshot).FilterNotNull().Map(a => a.Value());
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        public Event<TB> Collect<TB, TS>(TS initState, Func<TA, TS, Tuple<TB, TS>> snapshot)
        {
            var es = new Event<TS>();
            var s = es.Hold(initState);
            var ebs = Snapshot(s, snapshot);
            var eb = ebs.Map(bs => bs.Item1);
            var evt = ebs.Map(bs => bs.Item2);
            es.Loop(evt);
            return eb;
        }

        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        public Behavior<TS> Accum<TS>(TS initState, Func<TA, TS, TS> snapshot)
        {
            var loop = new Event<TS>();
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
            return new OnceEvent<TA>(this);
        }

        /// <summary>
        /// Stop all listeners from receiving events from the current Event
        /// </summary>
        public void Stop()
        {
            foreach (var l in listeners)
            {
                l.Stop();
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
        /// Fire the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="firing">The value to fire to registerd callbacks</param>
        internal virtual void Fire(Transaction transaction, TA firing)
        {
            var noFirings = !firings.Any();
            if (noFirings)
            {
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
            return TransactionContext.Run(t => this.Listen(t, callback, superior));
        }

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="transaction">Transaction to send any firings on</param>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An IListener to be used to stop listening for events.</returns>
        /// <remarks>Any firings that have occurred on the current transaction will be refired immediate after listening.</remarks>
        internal IListener Listen(Transaction transaction, ICallback<TA> callback, Rank superior)
        {
            RegisterCallback(transaction, callback, superior);
            InitialFire(transaction, callback);
            Refire(transaction, callback);
            return new Listener<TA>(this, callback, superior);
        }

        /// <summary>
        /// Similar to Listener, except that previous firings will not be refired.
        /// </summary>
        /// <param name="transaction">Transaction to send any firings on</param>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="superior">An IListener to be used to stop listening for events.</param>
        /// <returns>An IListener to be used to stop listening for events.</returns>
        /// <remarks>It's more common for the Listen method to be used instead of ListenSuppressed.
        /// You may want to use ListenSuppressed if the callback will be triggered as part of a call
        /// to Listen.
        /// 
        /// The only present use of ListenSuppressed is in the call chain of Behavior.SwitchE.</remarks>
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
                    transaction.Reprioritize = true;
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
            return new CoalesceEvent<TA>(this, coalesce, transaction);
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