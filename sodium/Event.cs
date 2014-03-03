namespace Sodium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An Event is a stream of discrete event occurrences
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the event.</typeparam>
    public class Event<T> : Observable<T>
    {
        /// <summary>
        /// List of IListeners that are currently listening for firings 
        /// from the current Event.
        /// </summary>
        private readonly List<EventListener<T>> listeners = new List<EventListener<T>>();

        /// <summary>
        /// List of values that have been fired on the current Event in the current transaction.
        /// Any listeners that are registered in the current transaction will get fired
        /// these values on registration.
        /// </summary>
        private readonly List<T> firings = new List<T>();

        /// <summary>
        /// The rank of the current Event. Default to rank zero
        /// </summary>
        private readonly Rank rank = new Rank();

        /// <summary>
        /// The current Rank of the Event, used to prioritize firings on the current transaction.
        /// </summary>
        internal Rank Rank
        {
            get
            {
                return this.rank;
            }
        }

        /// <summary>
        /// Cleanup the current Event, disposing of any listeners.
        /// </summary>
        public override void Dispose()
        {
            var clone = new List<EventListener<T>>(this.listeners);
            this.listeners.Clear();
            foreach (var listener in clone)
            {
                listener.Dispose();
            }

            base.Dispose();
        }

        /// <summary>
        /// Fire the given value to all registered listeners 
        /// </summary>
        /// <param name="a">The value to be fired</param>
        public override void Fire(T a)
        {
            TransactionContext.Current.Run(t => { Fire(t, a); return Unit.Nothing; });
        }

        /// <summary>
        /// Listen for firings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event fires.</param>
        /// <returns>An IListener, that should be Disposed when no longer needed. </returns>
        public override IEventListener<T> Listen(Action<T> callback)
        {
            return Listen(new SodiumCallback<T>((t, a) => callback(a)), Rank.Highest);
        }

        /// <summary>
        /// Similar to Listener, except that previous firings will not be re-fired.
        /// </summary>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <returns>An IListener to be used to stop listening for events.</returns>
        /// <remarks>It's more common for the Listen method to be used instead of ListenSuppressed.
        /// You may want to use ListenSuppressed if the action will be triggered as part of a call
        /// to Listen.</remarks>
        public override IEventListener<T> ListenSuppressed(Action<T> callback)
        {
            return ListenSuppressed(new SodiumCallback<T>((t, a) => callback(a)), Rank.Highest);
        }

        /// <summary>
        /// Map firings of the current event using the supplied mapping function.
        /// </summary>
        /// <param name="map">A map from T -> TB</param>
        public Event<TB> Map<TB>(Func<T, TB> map)
        {
            return new MapEvent<T, TB>(this, map);
        }

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        /// </summary>
        /// <returns>A Behavior that updates when the current event is fired,
        /// having the specified initial value.</returns>
        public Behavior<T> ToBehavior(T initValue)
        {
            return TransactionContext.Current.Run(t => ToBehavior(t, initValue));
        }

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        public Event<TC> Snapshot<TB, TC>(Behavior<TB> behavior, Func<T, TB, TC> snapshot)
        {
            return new SnapshotEvent<T, TB, TC>(this, snapshot, behavior);
        }

        /// <summary>
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        /// </summary>
        public Event<TB> Snapshot<TB>(Behavior<TB> behavior)
        {
            return Snapshot(behavior, (a, b) => b);
        }

        /// <summary>
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <param name="coalesce"></param>
        /// <remarks>
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        public Event<T> Coalesce(Func<T, T, T> coalesce)
        {
            return TransactionContext.Current.Run(t => Coalesce(t, coalesce));
        }

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        public Event<T> FilterNotNull()
        {
            return Filter(a => a != null);
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        public Event<T> Filter(Func<T, bool> predicate)
        {
            return new FilterEvent<T>(this, predicate);
        }

        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        public Behavior<TS> Accum<TS>(TS initState, Func<T, TS, TS> snapshot)
        {
            var evt = new EventLoop<TS>();
            var behavior = evt.ToBehavior(initState);
            var snapshotEvent = Snapshot(behavior, snapshot);
            evt.Loop(snapshotEvent);

            var result = snapshotEvent.ToBehavior(initState);
            result.RegisterFinalizer(evt);
            result.RegisterFinalizer(behavior);
            result.RegisterFinalizer(snapshotEvent);
            return result;
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
        public Event<T> Merge(Event<T> source2)
        {
            return new MergeEvent<T>(this, source2);
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
        public Event<T> Merge(Event<T> source2, Func<T, T, T> f)
        {
            var merge = this.Merge(source2);
            var c = merge.Coalesce(f);
            c.RegisterFinalizer(merge);
            return c;
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        public Event<T> Once()
        {
            return new OnceEvent<T>(this);
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        public Event<T> Delay()
        {
            return new DelayEvent<T>(this);
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        public Event<T> Gate(Behavior<bool> predicate)
        {
            Func<T, bool, Maybe<T>> snapshot = (a, p) => p ? new Maybe<T>(a) : null;
            var sn = this.Snapshot(predicate, snapshot);
            var filter = sn.FilterNotNull();
            var map = filter.Map(a => a.Value());
            map.RegisterFinalizer(filter);
            map.RegisterFinalizer(sn);
            return map;
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        public Event<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var es = new EventLoop<TS>();
            var s = es.ToBehavior(initState);
            var ebs = Snapshot(s, snapshot);
            var eb = ebs.Map(bs => bs.Item1);
            var evt = ebs.Map(bs => bs.Item2);
            es.Loop(evt);
            eb.RegisterFinalizer(es);
            eb.RegisterFinalizer(s);
            eb.RegisterFinalizer(ebs);
            eb.RegisterFinalizer(evt);
            return eb;
        }

        /// <summary>
        /// Fire the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="firing">The value to fire to registered callbacks</param>
        internal virtual void Fire(Transaction transaction, T firing)
        {
            ScheduleClearFirings(transaction);
            AddFiring(firing);
            InvokeCallbacks(transaction, firing);
        }

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="action">The action to invoke on a firing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An IListener to be used to stop listening for events</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// Listen operation that takes a thread. This ensures that any other
        /// actions triggered during Listen requiring a transaction all get the same instance.</remarks>
        internal IEventListener<T> Listen(ISodiumCallback<T> action, Rank superior)
        {
            return TransactionContext.Current.Run(t => this.Listen(t, action, superior));
        }

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="transaction">Transaction to send any firings on</param>
        /// <param name="action">The action to invoke on a firing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An IListener to be used to stop listening for events.</returns>
        /// <remarks>Any firings that have occurred on the current transaction will be re-fired immediate after listening.</remarks>
        internal EventListener<T> Listen(Transaction transaction, ISodiumCallback<T> action, Rank superior)
        {
            var listener = this.CreateListener(transaction, action, superior);
            InitialFire(transaction, listener);
            Refire(transaction, listener);
            return listener;
        }

        internal IEventListener<T> ListenSuppressed(Transaction transaction, ISodiumCallback<T> action, Rank superior)
        {
            var listener = this.CreateListener(transaction, action, superior);
            InitialFire(transaction, listener);
            return listener;
        }

        /// <summary>
        /// Clean up the output by discarding any firing other than the last one. 
        /// </summary>
        internal Event<T> LastFiringOnly(Transaction transaction)
        {
            return Coalesce(transaction, (a, b) => b);
        }

        internal IEventListener<T> ListenSuppressed(ISodiumCallback<T> action, Rank superior)
        {
            return TransactionContext.Current.Run(t => this.ListenSuppressed(t, action, superior));
        }

        /// <summary>
        /// Stop the given listener from receiving updates from the current Event
        /// </summary>
        /// <param name="eventListener">The listener to remove</param>
        /// <returns>True if the listener was removed, false otherwise</returns>
        internal bool RemoveListener(EventListener<T> eventListener)
        {
            if (eventListener == null)
            {
                return false;
            }

            lock (Constants.ListenersLock)
            {
                Rank.RemoveSuperior(eventListener.Rank);
                return this.listeners.Remove(eventListener);
            }
        }

        /// <summary>
        /// Gets the values that will be sent to newly added
        /// </summary>
        /// <returns>An Array of values that will be fired to all registered listeners</returns>
        protected internal virtual T[] InitialFirings()
        {
            return null;
        }

        private static void Fire(Transaction transaction, EventListener<T> eventListener, IEnumerable<T> firings)
        {
            foreach (var firing in firings)
            {
                eventListener.Callback.Invoke(transaction, firing);
            }
        }

        private EventListener<T> CreateListener(Transaction transaction, ISodiumCallback<T> action, Rank superior)
        {
            lock (Constants.ListenersLock)
            {
                if (this.rank.AddSuperior(superior))
                {
                    transaction.Reprioritize = true;
                }

                var listener = new EventListener<T>(this, action, superior);
                this.listeners.Add(listener);
                return listener;
            }
        }

        private void ScheduleClearFirings(Transaction transaction)
        {
            var noFirings = !firings.Any();
            if (noFirings)
            {
                transaction.Last(() => firings.Clear());
            }
        }

        private void AddFiring(T firing)
        {
            firings.Add(firing);
        }

        private void InvokeCallbacks(Transaction transaction, T firing)
        {
            var clone = new List<EventListener<T>>(this.listeners);
            foreach (var listener in clone)
            {
                if (listener.Callback != null)
                {
                    listener.Callback.Invoke(transaction, firing);
                }
            }
        }

        private void InitialFire(Transaction transaction, EventListener<T> listener)
        {
            var initialFirings = InitialFirings();
            if (initialFirings != null)
            {
                Fire(transaction, listener, initialFirings);
            }
        }

        /// <summary>
        /// Anything fired already in this transaction must be re-fired now so that
        /// there's no order dependency between send and listen.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="listener"></param>
        private void Refire(Transaction transaction, EventListener<T> listener)
        {
            Fire(transaction, listener, firings);
        }

        /// <summary>
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="coalesce"></param>
        /// <remarks>
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        private Event<T> Coalesce(Transaction transaction, Func<T, T, T> coalesce)
        {
            var evt = new CoalesceEvent<T>(this, coalesce, transaction);
            return evt;
        }

        private Behavior<T> ToBehavior(Transaction t, T initValue)
        {
            var f = LastFiringOnly(t);
            var b = new Behavior<T>(f, initValue);
            b.RegisterFinalizer(f);
            return b;
        }
    }
}