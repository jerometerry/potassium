namespace sodium
{
    internal class BehaviorApplyHandler<A, B> : IHandler<Transaction>
    {
        private bool _fired;
        private readonly EventSink<B> _ev;
        private readonly Behavior<ILambda1<A, B>> _bf;
        private readonly Behavior<A> _ba;

        public BehaviorApplyHandler(EventSink<B> ev, Behavior<ILambda1<A, B>> bf, Behavior<A> ba)
        {
            _ev = ev;
            _bf = bf;
            _ba = ba;
        }

        public void run(Transaction trans1)
        {
            if (_fired)
                return;

            _fired = true;
            trans1.Prioritized(_ev.Node, new HandlerImpl<Transaction>(t2 =>
            {
                var v = _bf.NewValue();
                var nv = _ba.NewValue();
                var b = v.apply(nv);
                _ev.send(t2, b);
                _fired = false;
            }));
        }
    }
}