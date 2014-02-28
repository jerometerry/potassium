namespace Sodium
{
    internal sealed class BehaviorValueEvent<TA> : Event<TA>
    {
        private Behavior<TA> behavior;
        private IEventListener<TA> listener;

        public BehaviorValueEvent(Behavior<TA> behavior, Event<TA> evt, Transaction transaction, bool allowAutoDispose)
            : base(allowAutoDispose)
        {
            this.behavior = behavior;
            var action = new SodiumAction<TA>(this.Fire);
            listener = evt.Listen(transaction, action, this.Rank, true);
        }

        protected internal override TA[] InitialFirings()
        {
            this.AssertNotDisposed();
            return new[] { behavior.Sample() };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (listener != null)
                {
                    listener.AutoDispose();
                    listener = null;
                }

                behavior = null;
            }

            base.Dispose(disposing);
        }
    }
}