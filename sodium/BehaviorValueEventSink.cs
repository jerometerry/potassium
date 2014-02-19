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

        public override TBehavior[] SampleNow()
        {
            return new TBehavior[] 
            { 
                _behavior.Sample()
            };
        }
    }
}