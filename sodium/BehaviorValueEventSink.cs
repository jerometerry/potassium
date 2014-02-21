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

        protected internal override Object[] SampleNow()
        {
            return new Object[] { _behavior.Sample() };
        }
    }
}