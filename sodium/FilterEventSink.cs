using System;

namespace sodium
{
    internal class FilterEventSink<A> : EventSink<A>
    {
        private Event<A> ev;
        private ILambda1<A, bool> f;

        public FilterEventSink(Event<A> ev, ILambda1<A, bool> f)
        {
            this.ev = ev;
            this.f = f;
        }

        protected internal override Object[] SampleNow()
        {
            Object[] oi = ev.SampleNow();
            if (oi != null)
            {
                Object[] oo = new Object[oi.Length];
                int j = 0;
                for (int i = 0; i < oi.Length; i++)
                    if (f.apply((A)oi[i]))
                        oo[j++] = oi[i];
                if (j == 0)
                    oo = null;
                else if (j < oo.Length)
                {
                    Object[] oo2 = new Object[j];
                    for (int i = 0; i < j; i++)
                        oo2[i] = oo[i];
                    oo = oo2;
                }
                return oo;
            }
            else
                return null;
        }
    }
}