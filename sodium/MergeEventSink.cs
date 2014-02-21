using System;

namespace sodium
{
    internal class MergeEventSink<A> : EventSink<A>
    {
        private Event<A> ea;
        private Event<A> eb;

        public MergeEventSink(Event<A> ea, Event<A> eb)
        {
            this.ea = ea;
            this.eb = eb;
        }

        protected internal override Object[] SampleNow()
        {
            Object[] oa = ea.SampleNow();
            Object[] ob = eb.SampleNow();
            if (oa != null && ob != null)
            {
                Object[] oo = new Object[oa.Length + ob.Length];
                int j = 0;
                for (int i = 0; i < oa.Length; i++) oo[j++] = oa[i];
                for (int i = 0; i < ob.Length; i++) oo[j++] = ob[i];
                return oo;
            }
            else
                if (oa != null)
                    return oa;
                else
                    return ob;
        }
    }
}