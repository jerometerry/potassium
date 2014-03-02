namespace Sodium
{
    internal class SwitchEvent<T> : Event<T>
    {
        private IEventListener<Event<T>> behaviorListener;
        private Behavior<Event<T>> sourceBehavior;
        private ISodiumCallback<T> wrappedEventListenerCallback;
        private IEventListener<T> wrappedEventListener;

        public SwitchEvent(Transaction transaction, Behavior<Event<T>> sourceBehavior)
        {
            this.sourceBehavior = sourceBehavior;

            this.wrappedEventListenerCallback = new SodiumCallback<T>(this.Fire);
            this.wrappedEventListener = sourceBehavior.Sample().Listen(transaction, this.wrappedEventListenerCallback, this.Rank);
            
            var behaviorEventChanged = new SodiumCallback<Event<T>>(UpdateWrappedEventListener);
            this.behaviorListener = sourceBehavior.Updates().Listen(transaction, behaviorEventChanged, this.Rank);
        }

        public override void Dispose()
        {
            if (this.behaviorListener != null)
            {
                this.behaviorListener.Dispose();
                this.behaviorListener = null;
            }

            if (this.wrappedEventListener != null)
            {
                this.wrappedEventListener.Dispose();
                this.wrappedEventListener = null;
            }

            this.sourceBehavior = null;

            base.Dispose();
        }

        public void UpdateWrappedEventListener(Transaction transaction, Event<T> newEvent)
        {
            transaction.Last(() =>
            {
                if (this.wrappedEventListener != null)
                {
                    this.wrappedEventListener.Dispose();
                    this.wrappedEventListener = null;
                }

                this.wrappedEventListener = newEvent.ListenSuppressed(transaction, this.wrappedEventListenerCallback, this.Rank);
            });
        }
    }
}
