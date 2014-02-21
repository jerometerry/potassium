using System;

namespace sodium
{
    internal class CoalesceEventSink<A> : EventSink<A>
    {
        private Event<A> ev;
        private ILambda2<A, A, A> f;

        public CoalesceEventSink(Event<A> ev, ILambda2<A, A, A> f)
        {
            this.ev = ev;
            this.f = f;
        }

        protected internal override Object[] SampleNow()
        {
            Object[] oi = ev.SampleNow();
            if (oi != null)
            {
                A o = (A)oi[0];
                for (int i = 1; i < oi.Length; i++)
                    o = f.apply(o, (A)oi[i]);
                return new Object[] { o };
            }
            else
                return null;
        }
    }
}