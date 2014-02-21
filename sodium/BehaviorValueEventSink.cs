using System;

namespace sodium
{
    internal class BehaviorValueEventSink<TA> : EventSink<TA>
    {
        private readonly Behavior<TA> _behavior;

        public BehaviorValueEventSink(Behavior<TA> behavior)
        {
            _behavior = behavior;
        }

        protected internal override TA[] SampleNow()
        {
            return new [] { _behavior.Sample() };
        }
    }
}