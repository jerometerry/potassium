namespace Sodium
{
    internal sealed class BehaviorValueEvent<TA> : Event<TA>
    {
        private readonly Behavior<TA> behavior;

        public BehaviorValueEvent(Behavior<TA> behavior, Event<TA> evt, Transaction transaction)
        {
            this.behavior = behavior;
            var callback = new Callback<TA>(this.Fire);
            evt.Listen(transaction, callback, this.Rank);
        }

        protected internal override TA[] InitialFirings()
        {
            return new[] { behavior.Sample() };
        }
    }
}