using System;

namespace sodium
{
    internal class SnapshotEventSink<A, B, C> : EventSink<C>
    {
        private Event<A> ev;
        private ILambda2<A, B, C> _f;
        private Behavior<B> b;

        public SnapshotEventSink(Event<A> ev, ILambda2<A, B, C> f, Behavior<B> b)
        {
            this.ev = ev;
            _f = f;
            this.b = b;
        }

        protected internal override Object[] SampleNow()
        {
            Object[] oi = ev.SampleNow();
            if (oi != null)
            {
                Object[] oo = new Object[oi.Length];
                for (int i = 0; i < oo.Length; i++)
                    oo[i] = _f.apply((A)oi[i], b.Sample());
                return oo;
            }
            else
                return null;
        }
    }
}