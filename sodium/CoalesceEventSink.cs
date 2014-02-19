namespace sodium
{
    using System;

    class CoalesceEventSink<TEvent> : EventSink<TEvent>
    {
        private readonly Event<TEvent> _event;
        private readonly IBinaryFunction<TEvent, TEvent, TEvent> _combiningFunction;

        public CoalesceEventSink(Event<TEvent> evt, IBinaryFunction<TEvent, TEvent, TEvent> combiningFunction)
        {
            _event = evt;
            _combiningFunction = combiningFunction;
        }

        internal override TEvent[] SampleNow()
        {
            var events = _event.SampleNow();
            if (events == null)
            {
                return null;
            }
            
            var evt = events[0];
            for (var i = 1; i < events.Length; i++)
                evt = _combiningFunction.Apply(evt, events[i]);
            return new TEvent[] { evt };
        }
    }
}
