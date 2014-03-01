namespace Sodium
{
    internal sealed class BehaviorValueEvent<TA> : Event<TA>
    {
        private Behavior<TA> behavior;
        private IEventListener<TA> listener;

        public BehaviorValueEvent(Behavior<TA> behavior, Transaction transaction)
        {
            this.behavior = behavior;
            var action = new SodiumAction<TA>(this.Fire);
            listener = behavior.Updates().Listen(transaction, action, this.Rank);
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            behavior = null;

            base.Dispose();
        }

        protected internal override TA[] InitialFirings()
        {
            return new[] { behavior.Sample() };
        }
    }
}