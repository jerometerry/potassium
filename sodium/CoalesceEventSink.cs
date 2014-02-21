using System;

namespace sodium
{
    internal class CoalesceEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> _ev;
        private readonly ILambda2<TA, TA, TA> _f;

        public CoalesceEventSink(Event<TA> ev, ILambda2<TA, TA, TA> f)
        {
            _ev = ev;
            _f = f;
        }

        protected internal override TA[] SampleNow()
        {
            var oi = _ev.SampleNow();
            if (oi != null)
            {
                var o = oi[0];
                for (var i = 1; i < oi.Length; i++)
                {
                    o = _f.Apply(o, oi[i]);
                }

                return new[] { o };
            }

            return null;
        }
    }
}