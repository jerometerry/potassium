namespace sodium
{
    using System;
    using System.Collections.Generic;

    public class Event<TA>
    {
        protected internal List<ITransactionHandler<TA>> Actions = new List<ITransactionHandler<TA>>();
        protected List<IListener> Listeners = new List<IListener>();
        internal Node Node = new Node(0L);
        protected List<TA> Firings = new List<TA>();

        protected internal virtual Object[] SampleNow()
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
            return Listen(new HandlerImpl<TA>(action));
        }

        /// <summary>
        /// Listen for firings of this event. The returned Listener has an unlisten()
        /// method to cause the listener to be removed. This is the observer pattern.
        ///</summary>
        public IListener Listen(IHandler<TA> action)
        {
            return Listen(Node.Null, new TransactionHandler<TA>((t, a) => action.run(a)));
        }

        internal IListener Listen(Node target, ITransactionHandler<TA> action)
        {
            return Transaction.Apply(new Lambda1Impl<Transaction, IListener>(t => Listen(target, t, action, false)));
        }

        internal IListener Listen(Node target, Transaction trans, ITransactionHandler<TA> action,
                                bool suppressEarlierFirings)
        {
            lock (Transaction.ListenersLock)
            {
                if (Node.LinkTo(target))
                    trans.ToRegen = true;
                Actions.Add(action);
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
                foreach (var a in Firings)
                    action.Run(trans, a);
            }
            return new Listener<TA>(this, action, target);
        }

        /// <summary>
        /// Overload of map that accepts a Func, allowing for C# lambda support
        /// </summary>
        /// <typeparam name="TB"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public Event<TB> Map<TB>(Func<TA, TB> f)
        {
            return Map(new Lambda1Impl<TA, TB>(f));
        }

        ///
        /// Transform the event's value according to the supplied function.
        ///
        public Event<TB> Map<TB>(ILambda1<TA, TB> f)
        {
            var ev = this;
            var sink = new MapEventSink<TA, TB>(ev, f);
            var l = Listen(sink.Node, new TransactionHandler<TA>((t, a) => sink.send(t, f.apply(a))));
            return sink.RegisterListener(l);
        }

        ///
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        ///
        public Behavior<TA> Hold(TA initValue)
        {
            return Transaction.Apply(new Lambda1Impl<Transaction, Behavior<TA>>(t => new Behavior<TA>(LastFiringOnly(t), initValue)));
        }

        ///
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        ///
        public Event<TB> Snapshot<TB>(Behavior<TB> beh)
        {
            return Snapshot(beh, new Lambda2Impl<TA, TB, TB>((a, b) => b));
        }

        ///
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        ///
        public Event<TC> Snapshot<TB, TC>(Behavior<TB> b, ILambda2<TA, TB, TC> f)
        {
            var ev = this;
            var sink = new SnapshotEventSink<TA, TB, TC>(ev, f, b);
            var l = Listen(sink.Node, new TransactionHandler<TA>((t2, a) => sink.send(t2, f.apply(a, b.Sample()))));
            return sink.RegisterListener(l);
        }

        ///
        /// Merge two streams of events of the same type.
        ///
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event firings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        ///
        public static Event<TA> Merge(Event<TA> ea, Event<TA> eb)
        {
            var sink = new MergeEventSink<TA>(ea, eb);
            var h = new TransactionHandler<TA>(sink.send);
            var l1 = ea.Listen(sink.Node, h);
            var l2 = eb.Listen(sink.Node, h);
            return sink.RegisterListener(l1).RegisterListener(l2);
        }

        ///
        /// Push each event occurrence onto a new transaction.
        ///
        public Event<TA> Delay()
        {
            var sink = new EventSink<TA>();
            var l1 = Listen(sink.Node, new TransactionHandler<TA>((t, a) => t.Post(new Runnable(() =>
            {
                var trans = new Transaction();
                try
                {
                    sink.send(trans, a);
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
            return Coalesce(new Lambda2Impl<TA, TA, TA>(f));
        }

        ///
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        ///
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        ///
        public Event<TA> Coalesce(ILambda2<TA, TA, TA> f)
        {
            return Transaction.Apply(new Lambda1Impl<Transaction, Event<TA>>(t => Coalesce(t, f)));
        }

        Event<TA> Coalesce(Transaction trans1, ILambda2<TA, TA, TA> f)
        {
            var ev = this;
            var sink = new CoalesceEventSink<TA>(ev, f);
            var h = new CoalesceHandler<TA>(f, sink);
            var l = Listen(sink.Node, trans1, h, false);
            return sink.RegisterListener(l);
        }

        ///
        /// Clean up the output by discarding any firing other than the last one. 
        ///
        internal Event<TA> LastFiringOnly(Transaction trans)
        {
            return Coalesce(trans, new Lambda2Impl<TA, TA, TA>((a, b) => b));
        }

        ///
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        ///
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        ///
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
            return Filter(new Lambda1Impl<TA, bool>(f));
        }

        ///
        /// Only keep event occurrences for which the predicate returns true.
        ///
        public Event<TA> Filter(ILambda1<TA, Boolean> f)
        {
            var ev = this;
            var sink = new FilterEventSink<TA>(ev, f);
            var l = Listen(sink.Node, new TransactionHandler<TA>((t, a) => { if (f.apply(a)) sink.send(t, a); }));
            return sink.RegisterListener(l);
        }

        ///
        /// Filter out any event occurrences whose value is a Java null pointer.
        ///
        public Event<TA> FilterNotNull()
        {
            return Filter(new Lambda1Impl<TA, Boolean>(a => a != null));
        }

        ///
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        ///
        public Event<TA> Gate(Behavior<Boolean> bPred)
        {
            var f = new Lambda2Impl<TA, bool, Maybe<TA>>((a, pred) => pred ? new Maybe<TA>(a) : null);
            return Snapshot(bPred, f).FilterNotNull().Map(a => a.Value());
        }

        ///
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        ///
        public Event<TB> Collect<TB, TS>(TS initState, ILambda2<TA, TS, Tuple2<TB, TS>> f)
        {
            var ea = this;
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
            var ebs = ea.Snapshot(s, f);
            var eb = ebs.Map(new Lambda1Impl<Tuple2<TB, TS>, TB>(bs => bs.V1));
            var esOut = ebs.Map(new Lambda1Impl<Tuple2<TB, TS>, TS>(bs => bs.V2));
            es.Loop(esOut);
            return eb;
        }


        public Behavior<TS> Accum<TS>(TS initState, Func<TA, TS, TS> f)
        {
            return Accum(initState, new Lambda2Impl<TA, TS, TS>(f));
        }

        ///
        /// Accumulate on input event, outputting the new state each time.
        ///
        public Behavior<TS> Accum<TS>(TS initState, ILambda2<TA, TS, TS> f)
        {
            var ea = this;
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
            var esOut = ea.Snapshot(s, f);
            es.Loop(esOut);
            return esOut.Hold(initState);
        }

        ///
        /// Throw away all event occurrences except for the first one.
        ///
        public Event<TA> Once()
        {
            // This is a bit long-winded but it's efficient because it deregisters
            // the listener.
            var ev = this;
            var la = new IListener[1];
            var sink = new OnceEventSink<TA>(ev, la);
            la[0] = ev.Listen(sink.Node, new TransactionHandler<TA>((t, a) =>
            {
                sink.send(t, a);
                if (la[0] == null) 
                    return;
                la[0].unlisten();
                la[0] = null;
            }));
            return sink.RegisterListener(la[0]);
        }

        internal Event<TA> RegisterListener(IListener listener)
        {
            Listeners.Add(listener);
            return this;
        }

        ~Event()
        {
            foreach (var l in Listeners)
                l.unlisten();
        }
    }
}