using System;

namespace sodium
{
    internal class FilterEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> _ev;
        private readonly ILambda1<TA, bool> _f;

        public FilterEventSink(Event<TA> ev, ILambda1<TA, bool> f)
        {
            _ev = ev;
            _f = f;
        }

        protected internal override TA[] SampleNow()
        {
            var oi = _ev.SampleNow();
            if (oi == null)
            {
                return null;
            }
            
            var results = new TA[oi.Length];
            var j = 0;
            foreach (var t in oi)
            {
                if (_f.Apply(t))
                    results[j++] = t;
            }
            if (j == 0)
            {
                results = null;
            }
            else if (j < results.Length)
            {
                var oo2 = new TA[j];
                for (var i = 0; i < j; i++)
                    oo2[i] = results[i];
                results = oo2;
            }
            return results;
        }
    }
}