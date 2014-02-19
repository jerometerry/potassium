namespace sodium
{
    using System;

    class BehaviorValueEventSink<TBehavior> : EventSink<TBehavior>
    {
        private readonly Behavior<TBehavior> _behavior;

        public BehaviorValueEventSink(Behavior<TBehavior> behavior)
        {
            _behavior = behavior;
        }

        internal override TBehavior[] SampleNow()
        {
            return new TBehavior[] 
            { 
                _behavior.Sample()
            };
        }
    }
}