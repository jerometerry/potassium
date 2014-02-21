namespace sodium
{
    internal class BehaviorApplyHandler<TA, TB> : IHandler<Transaction>
    {
        private bool _fired;
        private readonly EventSink<TB> _ev;
        private readonly Behavior<ILambda1<TA, TB>> _bf;
        private readonly Behavior<TA> _ba;

        public BehaviorApplyHandler(EventSink<TB> ev, Behavior<ILambda1<TA, TB>> bf, Behavior<TA> ba)
        {
            _ev = ev;
            _bf = bf;
            _ba = ba;
        }

        public void Run(Transaction t1)
        {
            if (_fired)
            { 
                return;
            }

            _fired = true;
            t1.Prioritized(_ev.Node, new Handler<Transaction>(t2 =>
            {
                var v = _bf.NewValue();
                var nv = _ba.NewValue();
                var b = v.Apply(nv);
                _ev.Send(t2, b);
                _fired = false;
            }));
        }
    }
}