using System;

namespace sodium
{
    internal class BehaviorValueEventSink<A> : EventSink<A>
    {
        private readonly Behavior<A> _behavior;

        public BehaviorValueEventSink(Behavior<A> behavior)
        {
            _behavior = behavior;
        }

        protected internal override Object[] SampleNow()
        {
            return new Object[] { _behavior.Sample() };
        }
    }
}