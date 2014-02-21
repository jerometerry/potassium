using System;

namespace sodium
{
    internal class OnceEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> _ev;
        private readonly IListener[] _la;

        public OnceEventSink(Event<TA> ev, IListener[] la)
        {
            _ev = ev;
            _la = la;
        }

        protected internal override Object[] SampleNow()
        {
            var oi = _ev.SampleNow();
            var oo = oi;
            if (oo != null)
            {
                if (oo.Length > 1)
                    oo = new[] { oi[0] };
                if (_la[0] != null)
                {
                    _la[0].Unlisten();
                    _la[0] = null;
                }
            }
            return oo;
        }
    }
}