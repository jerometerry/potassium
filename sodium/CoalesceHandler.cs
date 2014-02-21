namespace sodium
{
    internal class CoalesceHandler<TA> : ITransactionHandler<TA>
    {
        private readonly ILambda2<TA, TA, TA> _f;
        private readonly EventSink<TA> _ev;
        private Maybe<TA> _accum = Maybe<TA>.Null;

        public CoalesceHandler(ILambda2<TA, TA, TA> f, EventSink<TA> ev)
        {
            _f = f;
            _ev = ev;
        }

        public void Run(Transaction trans1, TA a)
        {
            if (_accum.HasValue)
            {
                _accum = new Maybe<TA>(_f.Apply(_accum.Value(), a));
            }
            else
            {
                var thiz = this;
                trans1.Prioritized(_ev.Node, new Handler<Transaction>(t =>
                {
                    _ev.Send(t, thiz._accum.Value());
                    thiz._accum = Maybe<TA>.Null;
                }));
                _accum = new Maybe<TA>(a);
            }
        }
    }
}