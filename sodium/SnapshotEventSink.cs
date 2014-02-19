namespace sodium
{
    using System;

    class SnapshotEventSink<TEvent, TBehavior, TSnapshot> : EventSink<TSnapshot>
    {
        private readonly Event<TEvent> _event;
        private readonly IBinaryFunction<TEvent, TBehavior, TSnapshot> _snapshotFunction;
        private readonly Behavior<TBehavior> _behavior;

        public SnapshotEventSink(
            Event<TEvent> ev, 
            IBinaryFunction<TEvent, TBehavior, TSnapshot> snapshotFunction, 
            Behavior<TBehavior> behavior)
        {
            _event = ev;
            _snapshotFunction = snapshotFunction;
            _behavior = behavior;
        }

        internal override TSnapshot[] SampleNow()
        {
            var inputs = _event.SampleNow();
            if (inputs == null)
            {
                return null;
            }

            var outputs = new TSnapshot[inputs.Length];
            for (int i = 0; i < outputs.Length; i++)
            { 
                var evt = inputs[i];
                var snapshot = _behavior.Sample();
                outputs[i] = _snapshotFunction.Apply(evt, snapshot);
            }
            return outputs;
        }
    }
}
