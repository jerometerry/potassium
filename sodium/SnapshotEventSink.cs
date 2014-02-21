using System;

namespace sodium
{
    internal class SnapshotEventSink<TA, TB, TC> : EventSink<TC>
    {
        private readonly Event<TA> _ev;
        private readonly ILambda2<TA, TB, TC> _f;
        private readonly Behavior<TB> _b;

        public SnapshotEventSink(Event<TA> ev, ILambda2<TA, TB, TC> f, Behavior<TB> b)
        {
            _ev = ev;
            _f = f;
            _b = b;
        }

        protected internal override TC[] SampleNow()
        {
            var oi = _ev.SampleNow();
            if (oi != null)
            {
                var oo = new TC[oi.Length];
                for (var i = 0; i < oo.Length; i++)
                    oo[i] = _f.Apply(oi[i], _b.Sample());
                return oo;
            }
            return null;
        }
    }
}