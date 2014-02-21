namespace sodium
{
    internal class CoalesceHandler<A> : ITransactionHandler<A>
    {
        private readonly ILambda2<A, A, A> _f;
        private readonly EventSink<A> _ev;
        private Maybe<A> _accum = Maybe<A>.Null;

        public CoalesceHandler(ILambda2<A, A, A> f, EventSink<A> ev)
        {
            _f = f;
            _ev = ev;
        }

        public void Run(Transaction trans1, A a)
        {
            if (_accum.HasValue)
                _accum = new Maybe<A>(_f.apply(_accum.Value(), a));
            else
            {
                var thiz = this;
                trans1.Prioritized(_ev.Node, new HandlerImpl<Transaction>(t =>
                {
                    _ev.send(t, thiz._accum.Value());
                    thiz._accum = Maybe<A>.Null;
                }));
                _accum = new Maybe<A>(a);
            }
        }
    }
}