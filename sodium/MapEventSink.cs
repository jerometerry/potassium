namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MapEventSink<TA, TB> : EventSink<TB>
    {
        private readonly Event<TA> evt;
        private readonly Func<TA, TB> f;

        public MapEventSink(Event<TA> evt, Func<TA, TB> f)
        {
            this.evt = evt;
            this.f = f;
        }

        protected internal override TB[] SampleNow()
        {
            var events = evt.SampleNow();
            if (events == null)
            { 
                return null;
            }

            return events.Select(e => f(e)).ToArray();
        }
    }
}
