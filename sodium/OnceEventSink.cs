using System;

namespace sodium
{
    internal class OnceEventSink<A> : EventSink<A>
    {
        private Event<A> ev;
        private IListener[] la;

        public OnceEventSink(Event<A> ev, IListener[] la)
        {
            this.ev = ev;
            this.la = la;
        }

        protected internal override Object[] SampleNow()
        {
            Object[] oi = ev.SampleNow();
            Object[] oo = oi;
            if (oo != null)
            {
                if (oo.Length > 1)
                    oo = new Object[] { oi[0] };
                if (la[0] != null)
                {
                    la[0].unlisten();
                    la[0] = null;
                }
            }
            return oo;
        }
    }
}