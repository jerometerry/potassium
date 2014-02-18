namespace sodium
{
    using System;

    class GetBehaviorValueEventSink<TBehavior> : EventSink<TBehavior>
    {
        private readonly Behavior<TBehavior> _behavior;

        public GetBehaviorValueEventSink(Behavior<TBehavior> behavior)
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