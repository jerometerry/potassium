namespace Sodium
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An Event is the observer pattern on steroids. The basic operations of Event are
    /// Listen and Fire. More interesting operations that you can perform on an Event 
    /// include Map, Filter, and ToBehavior, just to name a few.
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the event.</typeparam>
    /// <remarks>Events that fire in the same Transaction are known as Simultaneous Events.</remarks>
    public class Event<T> : Observable
    {
        /// <summary>
        /// List of IListeners that are currently listening for firings 
        /// from the current Event.
        /// </summary>
        private readonly List<IEventListener<T>> listeners = new List<IEventListener<T>>();

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
        /// List of IListeners that are currently listening for firings 
        /// from the current Event.
        /// </summary>
        protected List<IEventListener<T>> Listeners
        {
            get
            {
                return this.listeners;
            }
        }

        /// <summary>
        /// Listen for firings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event fires.</param>
        /// <returns>An IListener, that should be Disposed when no longer needed. </returns>
        public IEventListener<T> Listen(Action<T> callback)
        {
            return Listen(new ActionCallback<T>((a, t) => callback(a)), Rank.Highest);
        }

        /// <summary>
        /// Stop the given listener from receiving updates from the current Event
        /// </summary>
        /// <param name="eventListener">The listener to remove</param>
        /// <returns>True if the listener was removed, false otherwise</returns>
        public bool RemoveListener(IEventListener<T> eventListener)
        {
            if (eventListener == null)
            {
                return false;
            }

            var l = (EventListener<T>)eventListener;

            lock (Constants.ListenersLock)
            {
                Rank.RemoveSuperior(l.Rank);
                return this.Listeners.Remove(l);
            }
        }

        /// <summary>
        /// Map firings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that fires whenever the current Event fires, the
        /// the mapped value is computed using the supplied mapping.</returns>
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
        /// <param name="initValue">The initial value for the Behavior</param>
        /// <returns>A Behavior that updates when the current event is fired,
        /// having the specified initial value.</returns>
        public Behavior<T> Hold(T initValue)
        {
            return this.StartTransaction(t => this.Hold(initValue, t));
        }

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <param name="behavior">The Behavior to sample when calculating the snapshot</param>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <returns>A new Event that will produce the snapshot when the current event fires</returns>
        public Event<TC> Snapshot<TB, TC>(Behavior<TB> behavior, Func<T, TB, TC> snapshot)
        {
            return new SnapshotEvent<T, TB, TC>(this, snapshot, behavior);
        }

        /// <summary>
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <param name="behavior">The Behavior to sample when taking the snapshot</param>
        /// <returns>An event that captures the Behaviors value when the current event fires</returns>
        public Event<TB> Snapshot<TB>(Behavior<TB> behavior)
        {
            return Snapshot(behavior, (a, b) => b);
        }

        /// <summary>
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <param name="coalesce">A function that takes two firings of the same type, and returns
        /// produces a new firing of the same type.</param>
        /// <returns>A new Event that fires the coalesced values</returns>
        /// <remarks>
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        public Event<T> Coalesce(Func<T, T, T> coalesce)
        {
            return this.StartTransaction(t => Coalesce(coalesce, t));
        }

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <returns>A new Event that fires whenever the current Event fires with a non-null value</returns>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        public Event<T> FilterNotNull()
        {
            return Filter(a => a != null);
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        /// <param name="predicate">A predicate used to include firings</param>
        /// <returns>A new Event that is fired when the current Event fires and
        /// the predicate evaluates to true.</returns>
        public Event<T> Filter(Func<T, bool> predicate)
        {
            return new FilterEvent<T>(this, predicate);
        }

        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        /// <typeparam name="TS">The return type of the snapshot function</typeparam>
        /// <param name="initState">The initial state of the behavior</param>
        /// <param name="snapshot">The snapshot generation function</param>
        /// <returns>A new Behavior starting with the given value, that updates 
        /// whenever the current event fires, getting a value computed by the snapshot function.</returns>
        public Behavior<TS> Accum<TS>(TS initState, Func<T, TS, TS> snapshot)
        {
            var evt = new EventLoop<TS>();
            var behavior = evt.Hold(initState);
            var snapshotEvent = Snapshot(behavior, snapshot);
            evt.Loop(snapshotEvent);

            var result = snapshotEvent.Hold(initState);
            result.RegisterFinalizer(evt);
            result.RegisterFinalizer(behavior);
            result.RegisterFinalizer(snapshotEvent);
            return result;
        }

        /// <summary>
        /// Merge two streams of events of the same type.
        /// </summary>
        /// <param name="source">The Event to merge with the current Event</param>
        /// <returns>A new Event that fires whenever either the current or source Events fire.</returns>
        /// <remarks>
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event firings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </remarks>
        public Event<T> Merge(Event<T> source)
        {
            return new MergeEvent<T>(this, source);
        }

        /// <summary>
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        /// </summary>
        /// <param name="source">The Event to merge with the current Event</param>
        /// <param name="coalesce">The coalesce function that combines simultaneous firings.</param>
        /// <returns>An Event that is fired whenever the current or source Events fire, where
        /// simultaneous firings are handled by the coalesce function.</returns>
        /// <remarks>
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        /// </remarks>
        public Event<T> Merge(Event<T> source, Func<T, T, T> coalesce)
        {
            var merge = this.Merge(source);
            var c = merge.Coalesce(coalesce);
            c.RegisterFinalizer(merge);
            return c;
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        /// <returns>An Event that only fires one time, the first time the current event fires.</returns>
        public Event<T> Once()
        {
            return new OnceEvent<T>(this);
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        /// <returns>An event that is fired with the lowest priority in the current Transaction the current Event is fired in.</returns>
        public Event<T> Delay()
        {
            return new DelayEvent<T>(this);
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        /// <param name="predicate">A behavior who's current value acts as a predicate</param>
        /// <returns>A new Event that fires whenever the current Event fires and the Behaviors value
        /// is true.</returns>
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
        /// <typeparam name="TB">The return type of the new Event</typeparam>
        /// <typeparam name="TS">The snapshot type</typeparam>
        /// <param name="initState">The initial state for the internal Behavior</param>
        /// <param name="snapshot">The mealy machine</param>
        /// <returns>An Event that collects new values</returns>
        public Event<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
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

        internal virtual IEventListener<T> CreateListener(ISodiumCallback<T> source, Rank superior, Transaction transaction)
        {
            lock (Constants.ListenersLock)
            {
                if (this.rank.AddSuperior(superior))
                {
                    transaction.Reprioritize = true;
                }

                var listener = new EventListener<T>(this, source, superior);
                this.Listeners.Add(listener);
                return listener;
            }
        }

        /// <summary>
        /// Listen for firings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event fires.</param>
        /// <returns>An IListener, that should be Disposed when no longer needed. </returns>
        internal IEventListener<T> Listen(ISodiumCallback<T> callback)
        {
            return Listen(callback, Rank.Highest);
        }

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="listenerRank">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An IListener to be used to stop listening for events</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// Listen operation that takes a thread. This ensures that any other
        /// actions triggered during Listen requiring a transaction all get the same instance.</remarks>
        internal IEventListener<T> Listen(ISodiumCallback<T> callback, Rank listenerRank)
        {
            return this.StartTransaction(t => this.Listen(callback, listenerRank, t));
        }

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="transaction">Transaction to send any firings on</param>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An IListener to be used to stop listening for events.</returns>
        /// <remarks>Any firings that have occurred on the current transaction will be re-fired immediate after listening.</remarks>
        internal IEventListener<T> Listen(ISodiumCallback<T> callback, Rank superior, Transaction transaction)
        {
            return this.CreateListener(callback, superior, transaction);
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
        internal Behavior<T> Hold(T initValue, Transaction t)
        {
            var f = new LastFiringEvent<T>(this, t);
            var b = new Behavior<T>(f, initValue);
            b.RegisterFinalizer(f);
            return b;
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
        internal Event<T> Coalesce(Func<T, T, T> coalesce, Transaction transaction)
        {
            return new CoalesceEvent<T>(this, coalesce, transaction);
        }

        /// <summary>
        /// Cleanup the current Event, disposing of any listeners.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            var clone = new List<IEventListener<T>>(this.Listeners);
            this.Listeners.Clear();
            foreach (var listener in clone)
            {
                listener.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}