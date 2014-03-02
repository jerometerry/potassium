namespace Sodium
{
    internal sealed class SwitchEventAction<T> : SodiumObject, ISodiumAction<Event<T>>
    {
        private SwitchEvent<T> sourceEvent;
        private ISodiumAction<T> callback;
        private IEventListener<T> listener;
        private Behavior<Event<T>> behavior;

        public SwitchEventAction(Behavior<Event<T>> sourceBehavior, SwitchEvent<T> sourceEvent, Transaction t, ISodiumAction<T> callback)
        {
            this.behavior = sourceBehavior;
            this.sourceEvent = sourceEvent;
            this.callback = callback;
            this.listener = behavior.Sample().Listen(t, callback, sourceEvent.Rank);
        }

        public void Invoke(Transaction transaction, Event<T> newEvent)
        {
            transaction.Last(() =>
            {
                if (this.listener != null)
                { 
                    this.listener.Dispose();
                    this.listener = null;
                }

                this.listener = newEvent.ListenSuppressed(transaction, this.callback, sourceEvent.Rank);
            });
        }

        public override void Dispose()
        {
            if (this.listener != null)
            {
                this.listener.Dispose();
                this.listener = null;
            }

            sourceEvent = null;
            this.behavior = null;
            this.callback = null;

            base.Dispose();
        }
    }
}