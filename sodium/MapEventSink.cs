namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MapEventSink<TA, TB> : EventSink<TB>
    {
        private readonly Event<TA> evt;
        private readonly Func<TA, TB> map;

        public MapEventSink(Event<TA> evt, Func<TA, TB> map)
        {
            this.evt = evt;
            this.map = map;
        }

        public void MapAndSend(Transaction trans, TA firing)
        {
            this.Send(trans, this.map(firing));
        }

        protected internal override TB[] SampleNow()
        {
            var events = evt.SampleNow();
            if (events == null)
            { 
                return null;
            }

            return events.Select(e => this.map(e)).ToArray();
        }
    }
}
