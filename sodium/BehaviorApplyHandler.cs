namespace Sodium
{
    using System;

    internal sealed class BehaviorApplyHandler<TA, TB> 
    {
        private readonly EventSink<TB> evt;
        private readonly Behavior<Func<TA, TB>> bf;
        private readonly Behavior<TA> ba;
        private bool fired;

        public BehaviorApplyHandler(EventSink<TB> evt, Behavior<Func<TA, TB>> bf, Behavior<TA> ba)
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
            t1.Prioritized(evt.Node, Fire);
        }

        private void Fire(Transaction t)
        {
            var v = bf.NewValue();
            var nv = ba.NewValue();
            var b = v(nv);
            evt.Fire(t, b);
            fired = false;
        }
    }
}