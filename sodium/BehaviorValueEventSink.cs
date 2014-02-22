namespace Sodium
{
    using System;

    internal sealed class BehaviorValueEventSink<TA> : EventSink<TA>
    {
        private readonly Behavior<TA> behavior;

        public BehaviorValueEventSink(Behavior<TA> behavior)
        {
            this.behavior = behavior;
        }

        protected internal override TA[] SampleNow()
        {
            return new[] { behavior.Sample() };
        }
    }
}