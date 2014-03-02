namespace Sodium
{
    internal sealed class SwitchEventAction<T> : SodiumObject, ISodiumAction<Event<T>>
    {
        private SwitchEvent<T> sourceEvent;
        private ISodiumAction<T> action;
        private IEventListener<T> eventListener;
        private Behavior<Event<T>> bea;

        public SwitchEventAction(Behavior<Event<T>> sourceBehavior, SwitchEvent<T> sourceEvent, Transaction t, ISodiumAction<T> h)
        {
            this.bea = sourceBehavior;
            this.sourceEvent = sourceEvent;
            this.action = h;
            this.eventListener = sourceBehavior.Sample().Listen(t, h, sourceEvent.Rank);
        }

        public void Invoke(Transaction transaction, Event<T> newEvent)
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