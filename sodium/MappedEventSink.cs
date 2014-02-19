namespace sodium
{
    using System;

    class MappedEventSink<TEvent, TNewEvent> : EventSink<TNewEvent>
    {
        private readonly Event<TEvent> _event;
        private readonly IFunction<TEvent, TNewEvent> _mapFunction;

        public MappedEventSink(Event<TEvent> evt, IFunction<TEvent, TNewEvent> mapFunction)
        {
            _event = evt;
            _mapFunction = mapFunction;
        }

        internal override TNewEvent[] SampleNow()
        {
            var inputs = _event.SampleNow();
            if (inputs == null)
                return null;
            
            var outputs = new TNewEvent[inputs.Length];
            for (var i = 0; i < outputs.Length; i++)
                outputs[i] = _mapFunction.Apply(inputs[i]);
            return outputs;
            
        }
    }
}