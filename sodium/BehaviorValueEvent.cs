namespace Sodium
{
    internal sealed class BehaviorValueEvent<TA> : Event<TA>
    {
        private readonly Behavior<TA> behavior;

        public BehaviorValueEvent(Behavior<TA> behavior, Event<TA> evt, Transaction transaction)
        {
            this.behavior = behavior;
            var action = new SodiumAction<TA>(this.Fire);
            evt.Listen(transaction, action, this.Rank);
        }

        protected internal override TA[] InitialFirings()
        {
            return new[] { behavior.Sample() };
        }
    }
}