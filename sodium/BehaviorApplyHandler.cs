namespace Sodium
{
    internal sealed class BehaviorApplyHandler<TA, TB> : IHandler<Transaction>
    {
        private bool fired;
        private readonly EventSink<TB> evt;
        private readonly Behavior<ILambda1<TA, TB>> bf;
        private readonly Behavior<TA> ba;

        public BehaviorApplyHandler(EventSink<TB> evt, Behavior<ILambda1<TA, TB>> bf, Behavior<TA> ba)
        {
            this.evt = evt;
            this.bf = bf;
            this.ba = ba;
        }

        public void Run(Transaction t1)
        {
            if (fired)
            { 
                return;
            }

            fired = true;
            t1.Prioritized(evt.Node, new Handler<Transaction>(t2 =>
            {
                var v = bf.NewValue();
                var nv = ba.NewValue();
                var b = v.Apply(nv);
                evt.Send(t2, b);
                fired = false;
            }));
        }
    }
}