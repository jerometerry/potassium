using System;

namespace sodium
{
    internal class MergeEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> _ea;
        private readonly Event<TA> _eb;

        public MergeEventSink(Event<TA> ea, Event<TA> eb)
        {
            _ea = ea;
            _eb = eb;
        }

        protected internal override TA[] SampleNow()
        {
            var oa = _ea.SampleNow();
            var ob = _eb.SampleNow();
            if (oa != null && ob != null)
            {
                var oo = new TA[oa.Length + ob.Length];
                var j = 0;
                foreach (var t in oa)
                    oo[j++] = t;
                foreach (var t in ob)
                    oo[j++] = t;
                return oo;
            }

            return oa ?? ob;
        }
    }
}