namespace Sodium
{
    internal sealed class BehaviorValueEvent<TA> : Event<TA>
    {
        private Behavior<TA> behavior;
        private IEventListener<TA> listener;

        public BehaviorValueEvent(Behavior<TA> behavior, Event<TA> evt, Transaction transaction)
        {
            this.behavior = behavior;
            var action = new SodiumAction<TA>(this.Fire);
            listener = evt.Listen(transaction, action, this.Rank);
        }

        protected internal override TA[] InitialFirings()
        {
            return new[] { behavior.Sample() };
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
    }
}