namespace Sodium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Event<TA>
    {
        private readonly List<IHandler<TA>> actions = new List<IHandler<TA>>();
        private readonly List<IListener> listeners = new List<IListener>();
        private readonly Node node = new Node();
        private readonly List<TA> firings = new List<TA>();

        ~Event()
        {
            foreach (var l in listeners)
            {
                l.Unlisten();
            }
        }

        internal Node Node
        {
            get { return node; }
        }

        public static Event<TA> MergeWith(Func<TA, TA, TA> f, Event<TA> ea, Event<TA> eb)
        {
            return MergeWith(new Lambda2<TA, TA, TA>(f), ea, eb);
        }

        /// <summary>
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        ///
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        /// </summary>
        public static Event<TA> MergeWith(ILambda2<TA, TA, TA> f, Event<TA> ea, Event<TA> eb)
        {
            return Merge(ea, eb).Coalesce(f);
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
        public static Event<TA> Merge(Event<TA> ea, Event<TA> eb)
        {
            var sink = new MergeEventSink<TA>(ea, eb);
            var h = new Handler<TA>(sink.Send);
            var l1 = ea.Listen(sink.Node, h);
            var l2 = eb.Listen(sink.Node, h);
            return sink.RegisterListener(l1).RegisterListener(l2);
        }

        /// <summary>
        /// Listen for firings of this event. The returned Listener has an unlisten()
        /// method to cause the listener to be removed. This is the observer pattern.
        /// </summary>
        public IListener Listen(Action<TA> action)
        {
            return Listen(Node.Null, new Handler<TA>((t, a) => action(a)));
        }

        /// <summary>
        /// Overload of map that accepts a Func, allowing for C# lambda support
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public Event<TB> Map<TB>(Func<TA, TB> f)
        {
            return Map(new Lambda1<TA, TB>(f));
        }

        /// <summary>
        /// Transform the event's value according to the supplied function.
        /// </summary>
        public Event<TB> Map<TB>(ILambda1<TA, TB> f)
        {
            var ev = this;
            var sink = new MapEventSink<TA, TB>(ev, f);
            var l = Listen(sink.Node, new Handler<TA>((t, a) => sink.Send(t, f.Apply(a))));
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
        public Event<TB> Snapshot<TB>(Behavior<TB> beh)
        {
            return Snapshot(beh, new Lambda2<TA, TB, TB>((a, b) => b));
        }

        public Event<TC> Snapshot<TB, TC>(Behavior<TB> b, Func<TA, TB, TC> f)
        {
            return Snapshot(b, new Lambda2<TA, TB, TC>(f));
        }

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        public Event<TC> Snapshot<TB, TC>(Behavior<TB> b, ILambda2<TA, TB, TC> f)
        {
            var sink = new SnapshotEventSink<TA, TB, TC>(this, f, b);
            var handler = new Handler<TA>((t2, a) => sink.Send(t2, f.Apply(a, b.Sample())));
            var l = Listen(sink.Node, handler);
            return sink.RegisterListener(l);
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        public Event<TA> Delay()
        {
            var sink = new EventSink<TA>();
            var handler = new Handler<TA>((t, a) => t.Post(() => sink.Send(a)));
            var l1 = Listen(sink.Node, handler);
            return sink.RegisterListener(l1);
        }

        /// <summary>
        /// Overload of coalese that accepts a Func to support C# lambdas
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public Event<TA> Coalesce(Func<TA, TA, TA> f)
        {
            return Coalesce(new Lambda2<TA, TA, TA>(f));
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
        public Event<TA> Coalesce(ILambda2<TA, TA, TA> f)
        {
            return Transaction.Apply(t => Coalesce(t, f));
        }

        /// <summary>
        /// Overload of filter that accepts a Func to support C# lambda expressions
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public Event<TA> Filter(Func<TA, bool> f)
        {
            return Filter(new Lambda1<TA, bool>(f));
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        public Event<TA> Filter(ILambda1<TA, bool> predicate)
        {
            var sink = new FilterEventSink<TA>(this, predicate);
            var handler = new Handler<TA>((t, a) => sink.Send(predicate, t, a));
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
            var handler = new Lambda1<TA, bool>(a => a != null);
            return Filter(handler);
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        public Event<TA> Gate(Behavior<bool> predicate)
        {
            var f = new Lambda2<TA, bool, Maybe<TA>>((a, pred) => pred ? new Maybe<TA>(a) : null);
            return Snapshot(predicate, f).FilterNotNull().Map(a => a.Value());
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        public Event<TB> Collect<TB, TS>(TS initState, ILambda2<TA, TS, Tuple2<TB, TS>> f)
        {
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
            var ebs = Snapshot(s, f);
            var eb = ebs.Map(new Lambda1<Tuple2<TB, TS>, TB>(bs => bs.V1));
            var evt = ebs.Map(new Lambda1<Tuple2<TB, TS>, TS>(bs => bs.V2));
            es.Loop(evt);
            return eb;
        }

        public Behavior<TS> Accum<TS>(TS initState, Func<TA, TS, TS> f)
        {
            return Accum(initState, new Lambda2<TA, TS, TS>(f));
        }

        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        public Behavior<TS> Accum<TS>(TS initState, ILambda2<TA, TS, TS> f)
        {
            var ea = this;
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
            var evt = ea.Snapshot(s, f);
            es.Loop(evt);
            return evt.Hold(initState);
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
            la[0] = this.Listen(sink.Node, new Handler<TA>((t, a) => sink.Send(la, t, a)));
            return sink.RegisterListener(la[0]);
        }

        internal void Unlisten(IHandler<TA> action, Node target)
        {
            lock (Transaction.ListenersLock)
            {
                this.RemoveAction(action);
                this.Node.UnlinkTo(target);
            }
        }

        internal Event<TA> RegisterListener(IListener listener)
        {
            listeners.Add(listener);
            return this;
        }

        internal void RemoveAction(IHandler<TA> action)
        {
            actions.Remove(action);
        }

        internal void Send(Transaction trans, TA a)
        {
            if (!firings.Any())
            {
                trans.Last(() => firings.Clear());
            }

            firings.Add(a);

            var clonedActions = new List<IHandler<TA>>(actions);
            foreach (var action in clonedActions)
            {
                try
                {
                    action.Run(trans, a);
                }
                catch (Exception t)
                {
                    System.Diagnostics.Debug.WriteLine("{0}", t);
                }
            }
        }

        /// <summary>
        /// Clean up the output by discarding any firing other than the last one. 
        /// </summary>
        internal Event<TA> LastFiringOnly(Transaction trans)
        {
            return Coalesce(trans, new Lambda2<TA, TA, TA>((a, b) => b));
        }

        internal IListener Listen(Node target, IHandler<TA> action)
        {
            return Transaction.Apply(t => Listen(target, t, action, false));
        }

        internal IListener Listen(Node target, Transaction trans, IHandler<TA> action, bool suppressEarlierFirings)
        {
            lock (Transaction.ListenersLock)
            {
                trans.LinkNodes(this.node, target);
                actions.Add(action);
            }

            var events = SampleNow();
            if (events != null)
            {
                // In cases like value(), we start with an initial value.
                foreach (var t in events)
                {
                    action.Run(trans, t); // <-- unchecked warning is here
                }
            }

            if (!suppressEarlierFirings)
            {
                // Anything sent already in this transaction must be sent now so that
                // there's no order dependency between send and listen.
                foreach (var a in firings)
                {
                    action.Run(trans, a);
                }
            }

            return new Listener<TA>(this, action, target);
        }

        protected internal virtual TA[] SampleNow()
        {
            return null;
        }

        private Event<TA> Coalesce(Transaction t1, ILambda2<TA, TA, TA> f)
        {
            var ev = this;
            var sink = new CoalesceEventSink<TA>(ev, f);
            var h = new CoalesceHandler<TA>(sink, f);
            var l = Listen(sink.Node, t1, h, false);
            return sink.RegisterListener(l);
        }
    }
}