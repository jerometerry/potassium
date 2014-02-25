namespace Sodium
{
    using System;

    internal sealed class BehaviorValueEvent<TA> : Event<TA>
    {
        private readonly Behavior<TA> behavior;

        public BehaviorValueEvent(Behavior<TA> behavior)
        {
            this.behavior = behavior;
        }

        protected internal override TA[] InitialFirings()
        {
            return new[] { behavior.Sample() };
        }
    }
}