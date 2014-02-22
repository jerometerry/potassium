using System;

namespace Sodium
{
    public class MapEventSink<TA, TB> : EventSink<TB>
    {
        private readonly Event<TA> _ev;
        private readonly ILambda1<TA, TB> _f;

        public MapEventSink(Event<TA> ev, ILambda1<TA, TB> f)
        {
            _ev = ev;
            _f = f;
        }

        protected internal override TB[] SampleNow()
        {
            var oi = _ev.SampleNow();
            if (oi == null)
            { 
                return null;
            }
            
            var oo = new TB[oi.Length];
            for (int i = 0; i < oo.Length; i++)
            { 
                oo[i] = _f.Apply(oi[i]);
            }

            return oo;
        }
    }
}
