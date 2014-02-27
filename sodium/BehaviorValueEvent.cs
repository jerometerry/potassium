namespace Sodium
{
    internal sealed class BehaviorValueEvent<TA> : Event<TA>
    {
        private readonly Behavior<TA> behavior;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (listener != null)
                {
                    listener.Dispose();
                    listener = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}