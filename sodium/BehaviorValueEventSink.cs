namespace Sodium
{
    using System;

    internal sealed class BehaviorValueEventSink<TA> : Event<TA>
    {
        private readonly Behavior<TA> behavior;

        public BehaviorValueEventSink(Behavior<TA> behavior)
        {
            this.behavior = behavior;
        }

        protected internal override TA[] InitialFirings()
        {
            return new[] { behavior.Sample() };
        }
    }
}