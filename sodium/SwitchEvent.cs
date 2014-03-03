namespace Sodium
{
    internal class SwitchEvent<T> : Event<T>
    {
        private IEventListener<Event<T>> behaviorListener;
        private ISodiumCallback<T> wrappedEventListenerCallback;
        private IEventListener<T> wrappedEventListener;

        public SwitchEvent(Behavior<Event<T>> source, Transaction transaction)
        {
            this.wrappedEventListenerCallback = this.CreateFireCallback();
            this.wrappedEventListener = source.Sample().Listen(transaction, this.wrappedEventListenerCallback, this.Rank);
            
            var behaviorEventChanged = new ActionCallback<Event<T>>(UpdateWrappedEventListener);
            this.behaviorListener = source.Updates().Listen(transaction, behaviorEventChanged, this.Rank);
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

            this.wrappedEventListenerCallback = null;

            base.Dispose();
        }

        public void UpdateWrappedEventListener(Event<T> newEvent, Transaction transaction)
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
