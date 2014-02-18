using System.Linq;

namespace sodium
{
    using System;

    class FilteredEventSink<TEvent> : EventSink<TEvent>
    {
        private readonly Event<TEvent> _event;
        private readonly IFunction<TEvent, Boolean> _predicate;

        public FilteredEventSink(Event<TEvent> evt, IFunction<TEvent, Boolean> predicate)
        {
            _event = evt;
            _predicate = predicate;
        }

        public override TEvent[] SampleNow()
        {
            var inputs = _event.SampleNow();
            if (inputs == null)
            {
                return null;
            }

            var outputs = inputs.Where(_predicate.Apply).ToArray();

            if (outputs.Length == 0)
            {
                return null;
            }
            
            return outputs;
        }
    }
}
