namespace Sodium
{
    internal sealed class SwitchEventAction<TA> : SodiumItem, ISodiumAction<Event<TA>>
    {
        private SwitchEvent<TA> sourceEvent;
        private ISodiumAction<TA> action;
        private IEventListener<TA> eventListener;
        private Behavior<Event<TA>> bea;

        public SwitchEventAction(Behavior<Event<TA>> sourceBehavior, SwitchEvent<TA> sourceEvent, Transaction t, ISodiumAction<TA> h)
        {
            this.bea = sourceBehavior;
            this.sourceEvent = sourceEvent;
            this.action = h;
            this.eventListener = sourceBehavior.Sample().Listen(t, h, sourceEvent.Rank);
        }

        public void Invoke(Transaction transaction, Event<TA> newEvent)
        {
            transaction.Last(() =>
            {
                if (this.eventListener != null)
                { 
                    this.eventListener.Dispose();
                    this.eventListener = null;
                }

                this.eventListener = newEvent.ListenSuppressed(transaction, this.action, sourceEvent.Rank);
            });
        }

        public override void Dispose()
        {
            if (this.eventListener != null)
            {
                this.eventListener.Dispose();
                this.eventListener = null;
            }

            sourceEvent = null;
            bea = null;
            action = null;

            base.Dispose();
        }
    }
}