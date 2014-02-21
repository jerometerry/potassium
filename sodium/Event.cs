namespace sodium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Event<TA>
    {
        private readonly List<ITransactionHandler<TA>> _actions = new List<ITransactionHandler<TA>>();
        private readonly List<IListener> _listeners = new List<IListener>();
        private readonly Node _node = new Node();
        private readonly List<TA> _firings = new List<TA>();

        internal Node Node
        {
            get { return _node; }
        }

        internal void RemoveAction(ITransactionHandler<TA> action)
        {
            _actions.Remove(action);
        }

        internal void Send(Transaction trans, TA a)
        {
            if (!_firings.Any())
                trans.Last(new Runnable(() => _firings.Clear()));
            _firings.Add(a);

            var listeners = new List<ITransactionHandler<TA>>(_actions);
            foreach (var action in listeners)
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

        protected internal virtual TA[] SampleNow()
        {
            return null;
        }

        /// <summary>
        /// Overload of the listen method that accepts and Action, to support C# lambda expressions
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IListener Listen(Action<TA> action)
        {
            return Listen(new Handler<TA>(action));
        }

        /// <summary>
        /// Listen for firings of this event. The returned Listener has an unlisten()
        /// method to cause the listener to be removed. This is the observer pattern.
        ///</summary>
        public IListener Listen(IHandler<TA> action)
        {
            return Listen(Node.Null, new TransactionHandler<TA>((t, a) => action.Run(a)));
        }

        internal IListener Listen(Node target, ITransactionHandler<TA> action)
        {
            return Transaction.Apply(new Lambda1<Transaction, IListener>(t => Listen(target, t, action, false)));
        }

        internal IListener Listen(Node target, Transaction trans, ITransactionHandler<TA> action,
                                bool suppressEarlierFirings)
        {
            lock (Transaction.ListenersLock)
            {
                if (Node.LinkTo(target))
                    trans.ToRegen = true;
                _actions.Add(action);
            }
            var aNow = SampleNow();
            if (aNow != null)
            {
                // In cases like value(), we start with an initial value.
                foreach (var t in aNow)
                    action.Run(trans, (TA)t); // <-- unchecked warning is here
            }
            if (!suppressEarlierFirings)
            {
                // Anything sent already in this transaction must be sent now so that
                // there's no order dependency between send and listen.
                foreach (var a in _firings)
                    action.Run(trans, a);
            }
            return new Listener<TA>(this, action, target);
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
            var l = Listen(sink.Node, new TransactionHandler<TA>((t, a) => sink.Send(t, f.Apply(a))));
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
            return Transaction.Apply(new Lambda1<Transaction, Behavior<TA>>(t => new Behavior<TA>(LastFiringOnly(t), initValue)));
        }

        /// <summary>
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        /// </summary>
        public Event<TB> Snapshot<TB>(Behavior<TB> beh)
        {
            return Snapshot(beh, new Lambda2<TA, TB, TB>((a, b) => b));
        }

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        public Event<TC> Snapshot<TB, TC>(Behavior<TB> b, ILambda2<TA, TB, TC> f)
        {
            var ev = this;
            var sink = new SnapshotEventSink<TA, TB, TC>(ev, f, b);
            var l = Listen(sink.Node, new TransactionHandler<TA>((t2, a) => sink.Send(t2, f.Apply(a, b.Sample()))));
            return sink.RegisterListener(l);
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
            var h = new TransactionHandler<TA>(sink.Send);
            var l1 = ea.Listen(sink.Node, h);
            var l2 = eb.Listen(sink.Node, h);
            return sink.RegisterListener(l1).RegisterListener(l2);
        }

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        public Event<TA> Delay()
        {
            var sink = new EventSink<TA>();
            var l1 = Listen(sink.Node, new TransactionHandler<TA>((t, a) => t.Post(new Runnable(() =>
            {
                var trans = new Transaction();
                try
                {
                    sink.Send(trans, a);
                }
                finally
                {
                    trans.Close();
                }
            }))));

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
            return Transaction.Apply(new Lambda1<Transaction, Event<TA>>(t => Coalesce(t, f)));
        }

        Event<TA> Coalesce(Transaction t1, ILambda2<TA, TA, TA> f)
        {
            var ev = this;
            var sink = new CoalesceEventSink<TA>(ev, f);
            var h = new CoalesceHandler<TA>(f, sink);
            var l = Listen(sink.Node, t1, h, false);
            return sink.RegisterListener(l);
        }

        /// <summary>
        /// Clean up the output by discarding any firing other than the last one. 
        /// </summary>
        internal Event<TA> LastFiringOnly(Transaction trans)
        {
            return Coalesce(trans, new Lambda2<TA, TA, TA>((a, b) => b));
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
        public Event<TA> Filter(ILambda1<TA, Boolean> f)
        {
            var ev = this;
            var sink = new FilterEventSink<TA>(ev, f);
            var l = Listen(sink.Node, new TransactionHandler<TA>((t, a) => { if (f.Apply(a)) sink.Send(t, a); }));
            return sink.RegisterListener(l);
        }

        /// <summary>
        /// Filter out any event occurrences whose value is a Java null pointer.
        /// </summary>
        public Event<TA> FilterNotNull()
        {
            return Filter(new Lambda1<TA, Boolean>(a => a != null));
        }

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        public Event<TA> Gate(Behavior<Boolean> bPred)
        {
            var f = new Lambda2<TA, bool, Maybe<TA>>((a, pred) => pred ? new Maybe<TA>(a) : null);
            return Snapshot(bPred, f).FilterNotNull().Map(a => a.Value());
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        public Event<TB> Collect<TB, TS>(TS initState, ILambda2<TA, TS, Tuple2<TB, TS>> f)
        {
            var ea = this;
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
            var ebs = ea.Snapshot(s, f);
            var eb = ebs.Map(new Lambda1<Tuple2<TB, TS>, TB>(bs => bs.V1));
            var esOut = ebs.Map(new Lambda1<Tuple2<TB, TS>, TS>(bs => bs.V2));
            es.Loop(esOut);
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
            var esOut = ea.Snapshot(s, f);
            es.Loop(esOut);
            return esOut.Hold(initState);
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        public Event<TA> Once()
        {
            // This is a bit long-winded but it's efficient because it deregisters
            // the listener.
            var ev = this;
            var la = new IListener[1];
            var sink = new OnceEventSink<TA>(ev, la);
            la[0] = ev.Listen(sink.Node, new TransactionHandler<TA>((t, a) =>
            {
                sink.Send(t, a);
                if (la[0] == null) 
                    return;
                la[0].Unlisten();
                la[0] = null;
            }));
            return sink.RegisterListener(la[0]);
        }

        internal Event<TA> RegisterListener(IListener listener)
        {
            _listeners.Add(listener);
            return this;
        }

        ~Event()
        {
            foreach (var l in _listeners)
                l.Unlisten();
        }
    }
}