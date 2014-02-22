namespace Sodium
{
    internal sealed class CoalesceHandler<TA> : ITransactionHandler<TA>
    {
        private readonly ILambda2<TA, TA, TA> f;
        private readonly EventSink<TA> evt;
        private Maybe<TA> accum = Maybe<TA>.Null;

        public CoalesceHandler(EventSink<TA> evt, ILambda2<TA, TA, TA> f)
        {
            this.evt = evt;
            this.f = f;
        }

        public void Run(Transaction t1, TA a)
        {
            if (accum.HasValue)
            {
                accum = new Maybe<TA>(f.Apply(accum.Value(), a));
            }
            else
            {
                var handler = this;
                t1.Prioritized(evt.Node, new Handler<Transaction>(t2 =>
                {
                    evt.Send(t2, handler.accum.Value());
                    handler.accum = Maybe<TA>.Null;
                }));
                accum = new Maybe<TA>(a);
            }
        }
    }
}