namespace Sodium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An Event is a stream of discrete event occurrences
    /// </summary>
    /// <typeparam name="TA">The type of values that will be fired through the event.</typeparam>
    public class Event<TA> : IDisposable
    {
        private readonly List<Listener<TA>> listeners = new List<Listener<TA>>();
        private readonly List<TA> firings = new List<TA>();
        
        /// <summary>
        /// The rank of the current Event. Default to rank zero
        /// </summary>
        private readonly Rank rank = new Rank();

        private Event<TA> loop;
        private bool disposed;

        /// <summary>
        /// Gets whether the current Event has been disposed
        /// </summary>
        public bool Disposed
        {
            get { return disposed; }
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
        
        /// <summary>
        /// Firings on le will be forwarded to the current Event
        /// </summary>
        /// <param name="le">Event who's firings will be looped to the current Event</param>
        /// <remarks>Loop can only be called once on an Event. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public void Loop(Event<TA> le)
        {
            AssertNotDisposed();

            if (this.loop != null)
            {
                throw new ApplicationException("EventLoop looped more than once");
            }

            this.loop = le;
            var evt = this;
            le.Listen(new SodiumAction<TA>(evt.Fire), evt.Rank);
        }

        /// <summary>
        /// Fire the given value to all registered listeners 
        /// </summary>
        /// <param name="a">The value to be fired</param>
        public void Fire(TA a)
        {
            AssertNotDisposed();
            TransactionContext.Current.Run(t => { Fire(t, a); return Unit.Value; });
        }

        /// <summary>
        /// Listen for firings of this event. The returned Listener has an Unlisten()
        /// method to cause the listener to be removed. This is the observer pattern.
        /// </summary>
        /// <param name="action">An Action to be invoked when the current Event fires.</param>
        /// <returns>An IListener </returns>
        public IListener<TA> Listen(Action<TA> action)
        {
            AssertNotDisposed();
            return Listen(new SodiumAction<TA>((t, a) => action(a)), Rank.Highest);
        }

        /// <summary>
        /// Similar to Listener, except that previous firings will not be re-fired.
        /// </summary>
        /// <param name="action">The action to invoke on a firing</param>
        /// <returns>An IListener to be used to stop listening for events.</returns>
        /// <remarks>It's more common for the Listen method to be used instead of ListenSuppressed.
        /// You may want to use ListenSuppressed if the action will be triggered as part of a call
        /// to Listen.</remarks>
        public IListener<TA> ListenSuppressed(Action<TA> action)
        {
            AssertNotDisposed();
            return ListenSuppressed(new SodiumAction<TA>((t, a) => action(a)), Rank.Highest);
        }

        /// <summary>
        /// Map firings of the current event using the supplied mapping function.
        /// </summary>
        /// <param name="map">A map from TA -> TB</param>
        public Event<TB> Map<TB>(Func<TA, TB> map)
        {
            AssertNotDisposed();
            return new MapEvent<TA, TB>(this, map);
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
        public Behavior<TA> Hold(TA initValue)
        {
            AssertNotDisposed();
            return TransactionContext.Current.Run(t => new Behavior<TA>(LastFiringOnly(t), initValue));
        }

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        /// </summary>
        /// <param name="initValue"></param>
        /// <param name="transaction"></param>
        /// <returns>A Behavior that updates when the current event is fired,
        /// having the specified initial value.</returns>
        public Behavior<TA> Hold(TA initValue, Transaction transaction)
        {
            AssertNotDisposed();
            return new Behavior<TA>(LastFiringOnly(transaction), initValue);
        }

        /// <summary>
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        /// </summary>
        public Event<TB> Snapshot<TB>(Behavior<TB> behavior)
        {
            AssertNotDisposed();
            return Snapshot(behavior, (a, b) => b);
        }

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        public Event<TC> Snapshot<TB, TC>(Behavior<TB> behavior, Func<TA, TB, TC> snapshot)
        {
            AssertNotDisposed();
            return new SnapshotEvent<TA, TB, TC>(this, snapshot, behavior);
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        public Event<TA> Delay()
        {
            AssertNotDisposed();
            return new DelayEvent<TA>(this);
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
        public Event<TA> Coalesce(Func<TA, TA, TA> coalesce)
        {
            AssertNotDisposed();
            return TransactionContext.Current.Run(t => Coalesce(t, coalesce));
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
        public Event<TA> Coalesce(Transaction transaction, Func<TA, TA, TA> coalesce)
        {
            AssertNotDisposed();
            return new CoalesceEvent<TA>(this, coalesce, transaction);
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        public Event<TA> Filter(Func<TA, bool> predicate)
        {
            AssertNotDisposed();
            return new FilterEvent<TA>(this, predicate);
        }

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        public Event<TA> FilterNotNull()
        {
            AssertNotDisposed();
            return Filter(a => a != null);
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        public Event<TA> Gate(Behavior<bool> predicate)
        {
            AssertNotDisposed();
            Func<TA, bool, Maybe<TA>> snapshot = (a, p) => p ? new Maybe<TA>(a) : null;
            return this.Snapshot(predicate, snapshot).FilterNotNull().Map(a => a.Value());
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        public Event<TB> Collect<TB, TS>(TS initState, Func<TA, TS, Tuple<TB, TS>> snapshot)
        {
            AssertNotDisposed();
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
            AssertNotDisposed();
            var evt = new Event<TS>();
            var behavior = evt.Hold(initState);
            var snapshotEvent = Snapshot(behavior, snapshot);
            evt.Loop(snapshotEvent);
            return snapshotEvent.Hold(initState);
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        public Event<TA> Once()
        {
            AssertNotDisposed();
            return new OnceEvent<TA>(this);
        }

        /// <summary>
        /// Stop all listeners from receiving events from the current Event
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up the output by discarding any firing other than the last one. 
        /// </summary>
        internal Event<TA> LastFiringOnly(Transaction transaction)
        {
            return Coalesce(transaction, (a, b) => b);
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

            var clone = new List<Listener<TA>>(this.listeners);
            foreach (var listener in clone)
            {
                listener.Action.Invoke(transaction, firing);
            }
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
        internal IListener<TA> Listen(ISodiumAction<TA> action, Rank superior)
        {
            return TransactionContext.Current.Run(t => this.Listen(t, action, superior));
        }

        internal IListener<TA> ListenSuppressed(ISodiumAction<TA> action, Rank superior)
        {
            return TransactionContext.Current.Run(t => this.ListenSuppressed(t, action, superior));
        }

        internal IListener<TA> Listen(Transaction transaction, Action<TA> action)
        {
            AssertNotDisposed();
            return Listen(transaction, new SodiumAction<TA>((t, a) => action(a)), Rank.Highest);
        }

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="transaction">Transaction to send any firings on</param>
        /// <param name="action">The action to invoke on a firing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An IListener to be used to stop listening for events.</returns>
        /// <remarks>Any firings that have occurred on the current transaction will be re-fired immediate after listening.</remarks>
        internal IListener<TA> Listen(Transaction transaction, ISodiumAction<TA> action, Rank superior)
        {
            var listener = this.CreateListener(transaction, action, superior);
            InitialFire(transaction, listener);
            Refire(transaction, listener);
            return listener;
        }

        internal IListener<TA> ListenSuppressed(Transaction transaction, Action<TA> action)
        {
            AssertNotDisposed();
            return ListenSuppressed(transaction, new SodiumAction<TA>((t, a) => action(a)), Rank.Highest);
        }

        internal IListener<TA> ListenSuppressed(Transaction transaction, ISodiumAction<TA> action, Rank superior)
        {
            var listener = this.CreateListener(transaction, action, superior);
            InitialFire(transaction, listener);
            return listener;
        }

        /// <summary>
        /// Stop the given listener from receiving updates from the current Event
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        /// <returns>True if the listener was removed, false otherwise</returns>
        internal bool RemoveListener(Listener<TA> listener)
        {
            AssertNotDisposed();
            if (listener == null || listener.Disposed)
            {
                return false;
            }

            lock (Constants.ListenersLock)
            {
                Rank.RemoveSuperior(listener.Rank);
                return this.listeners.Remove(listener);
            }
        }

        /// <summary>
        /// Gets the values that will be sent to newly added
        /// </summary>
        /// <returns>An Array of values that will be fired to all registered listeners</returns>
        protected internal virtual TA[] InitialFirings()
        {
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var listener in this.listeners)
                {
                    listener.Dispose();
                }

                this.listeners.Clear();
            }

            disposed = true;
        }

        private static void Fire(Transaction transaction, Listener<TA> listener, IEnumerable<TA> firings)
        {
            foreach (var firing in firings)
            {
                listener.Action.Invoke(transaction, firing);
            }
        }

        private void AssertNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Event is being used after it's disposed");
            }
        }

        private Listener<TA> CreateListener(Transaction transaction, ISodiumAction<TA> action, Rank superior)
        {
            lock (Constants.ListenersLock)
            {
                if (this.rank.AddSuperior(superior))
                {
                    transaction.Reprioritize = true;
                }

                var listener = new Listener<TA>(this, action, superior);
                this.listeners.Add(listener);
                return listener;
            }
        }

        private void InitialFire(Transaction transaction, Listener<TA> listener)
        {
            var initialFirings = InitialFirings();
            if (initialFirings != null)
            {
                Fire(transaction, listener, initialFirings);
            }
        }

        /// <summary>
        /// Anything fired already in this transaction must be refired now so that
        /// there's no order dependency between send and listen.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="listener"></param>
        private void Refire(Transaction transaction, Listener<TA> listener)
        {
            Fire(transaction, listener, firings);
        }
    }
}