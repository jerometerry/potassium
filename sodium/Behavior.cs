namespace sodium
{
    using System;

    public class Behavior<A>
    {
        protected Event<A> Event;
        protected A Val;
        Maybe<A> _valueUpdate = Maybe<A>.Null;
        private IListener _cleanup;

        ///
        /// A behavior with a constant value.
        ///
        public Behavior(A val)
        {
            Event = new Event<A>();
            Val = val;
        }

        internal Behavior(Event<A> evt, A initVal)
        {
            Event = evt;
            Val = initVal;
            var behavior = this;

            Transaction.Run(new HandlerImpl<Transaction>(t1 =>
            {
                var handler = new TransactionHandler<A>((t2, a) =>
                {
                    if (!behavior._valueUpdate.HasValue)
                    {
                        t2.Last(new Runnable(() =>
                        {
                            behavior.Val = behavior._valueUpdate.Value();
                            behavior._valueUpdate = Maybe<A>.Null;
                        }));
                    }
                    _valueUpdate = new Maybe<A>(a);

                });
                _cleanup = evt.Listen(Node.Null, t1, handler, false);
            }));
        }

        ///
        /// @return The value including any updates that have happened in this transaction.
        ///
        internal A NewValue()
        {
            return !_valueUpdate.HasValue ? Val : _valueUpdate.Value();
        }

        ///
        /// Sample the behavior's current value.
        ///
        /// This should generally be avoided in favour of value().listen(..) so you don't
        /// miss any updates, but in many circumstances it makes sense.
        ///
        /// It can be best to use it inside an explicit transaction (using Transaction.run()).
        /// For example, a b.sample() inside an explicit transaction along with a
        /// b.updates().listen(..) will capture the current value and any updates without risk
        /// of missing any in between.
        ///
        public A Sample()
        {
            // Since pointers in Java are atomic, we don't need to explicitly create a
            // transaction.
            return Val;
        }

        ///
        /// An event that gives the updates for the behavior. If this behavior was created
        /// with a hold, then updates() gives you an event equivalent to the one that was held.
        ///
        public Event<A> Updates()
        {
            return Event;
        }

        ///
        /// An event that is guaranteed to fire once when you listen to it, giving
        /// the current value of the behavior, and thereafter behaves like updates(),
        /// firing for each update to the behavior's value.
        ///
        public Event<A> Value()
        {
            return Transaction.Apply(new Lambda1Impl<Transaction, Event<A>>(Value));
        }

        internal Event<A> Value(Transaction trans1)
        {
            var out_ = new BehaviorValueEventSink<A>(this);
            var l = Event.Listen(out_.Node, trans1,
                new TransactionHandler<A>(out_.send), false);
            return out_.RegisterListener(l)
                .LastFiringOnly(trans1);  // Needed in case of an initial value and an update
                                          // in the same transaction.
        }

        /// <summary>
        /// Overload of map that accepts a Func<A,B> to support C# lambdas
        /// </summary>
        /// <typeparam name="B"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public Behavior<B> Map<B>(Func<A, B> f)
        {
            return Map(new Lambda1Impl<A, B>(f));
        }

        ///
        /// Transform the behavior's value according to the supplied function.
        ///
        public Behavior<B> Map<B>(ILambda1<A, B> f)
        {
            return Updates().Map(f).Hold(f.apply(Sample()));
        }

        ///
        /// Lift a binary function into behaviors.
        ///
        public Behavior<C> Lift<B, C>(ILambda2<A, B, C> f, Behavior<B> b)
        {
            var ffa = new Lambda1Impl<A, ILambda1<B, C>>((aa) => new Lambda1Impl<B, C>((bb) => f.apply(aa, bb)));
            var bf = Map(ffa);
            return Apply(bf, b);
        }

        /// <summary>
        /// Overload of lift that accepts binary function Func<A,B,C> f and two behaviors, to enable C# lambdas
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Behavior<C> Lift<A, B, C>(Func<A, B, C> f, Behavior<A> a, Behavior<B> b)
        {
            return Lift(new Lambda2Impl<A, B, C>(f), a, b);
        }

        ///
        /// Lift a binary function into behaviors.
        ///
        public static Behavior<C> Lift<A, B, C>(ILambda2<A, B, C> f, Behavior<A> a, Behavior<B> b)
        {
            return a.Lift(f, b);
        }

        ///
        /// Lift a ternary function into behaviors.
        ///
        // TODO
        //public Behavior<D> Lift<B, C, D>(Lambda3<A, B, C, D> f, Behavior<B> b, Behavior<C> c)
        //{
        //    
        //    Lambda1<A, Lambda1<B, Lambda1<C, D>>> ffa = null;
        //    //Lambda1<A, Lambda1<B, Lambda1<C,D>>> ffa = new Lambda1<A, Lambda1<B, Lambda1<C,D>>>() {
        //    //    public Lambda1<B, Lambda1<C,D>> apply(final A aa) {
        //    //        return new Lambda1<B, Lambda1<C,D>>() {
        //    //            public Lambda1<C,D> apply(final B bb) {
        //    //                return new Lambda1<C,D>() {
        //    //                    public D apply(C cc) {
        //    //                        return f.apply(aa,bb,cc);
        //    //                    }
        //    //                };
        //    //            }
        //    //        };
        //    //    }
        //    //};
        //    Behavior<Lambda1<B, Lambda1<C, D>>> bf = map(ffa);
        //    return apply(apply(bf, b), c);
        //}

        ///
        /// Lift a ternary function into behaviors.
        ///
        // TODO
        //public static Behavior<D> Lift<A, B, C, D>(Lambda3<A, B, C, D> f, Behavior<A> a, Behavior<B> b, Behavior<C> c)
        //{
        //    return a.lift(f, b, c);
        //}

        ///
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        ///
        public static Behavior<B> Apply<A, B>(Behavior<ILambda1<A, B>> bf, Behavior<A> ba)
        {
            var out_ = new EventSink<B>();
            var h = new BehaviorApplyHandler<A, B>(out_, bf, ba);
            var l1 = bf.Updates().Listen(out_.Node, new TransactionHandler<ILambda1<A, B>>((t, f) => h.run(t)));
            var l2 = ba.Updates().Listen(out_.Node, new TransactionHandler<A>((t, a) => h.run(t)));
            return out_.RegisterListener(l1).RegisterListener(l2).Hold(bf.Sample().apply(ba.Sample()));
        }

        ///
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        ///
        public static Behavior<A> SwitchB<A>(Behavior<Behavior<A>> bba)
        {
            var za = bba.Sample().Sample();
            var out_ = new EventSink<A>();
            var h = new BehaviorSwitchHandler<A>(out_);
            var l1 = bba.Value().Listen(out_.Node, h);
            return out_.RegisterListener(l1).Hold(za);
        }

        ///
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        ///
        public static Event<A> SwitchE<A>(Behavior<Event<A>> bea)
        {
            return Transaction.Apply(new Lambda1Impl<Transaction, Event<A>>((t) => SwitchE<A>(t, bea)));
        }

        private static Event<A> SwitchE<A>(Transaction trans1, Behavior<Event<A>> bea)
        {
            var out_ = new EventSink<A>();
            var h2 = new TransactionHandler<A>(out_.send);
            var h1 = new EventSwitchHandler<A>(bea, out_, trans1, h2);
            var l1 = bea.Updates().Listen(out_.Node, trans1, h1, false);
            return out_.RegisterListener(l1);
        }

        ///
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        ///
        public Behavior<B> Collect<B, S>(S initState, ILambda2<A, S, Tuple2<B, S>> f)
        {
            var ea = Updates().Coalesce(new Lambda2Impl<A, A, A>((a, b) => b));
            var za = Sample();
            var zbs = f.apply(za, initState);
            var ebs = new EventLoop<Tuple2<B, S>>();
            var bbs = ebs.Hold(zbs);
            var bs = bbs.Map(new Lambda1Impl<Tuple2<B, S>, S>(x => x.V2));
            var ebs_out = ea.Snapshot(bs, f);
            ebs.loop(ebs_out);
            return bbs.Map(new Lambda1Impl<Tuple2<B, S>, B>(x => x.V1));
        }

        ~Behavior()
        {
            if (_cleanup != null)
                _cleanup.unlisten();
        }

    }
}