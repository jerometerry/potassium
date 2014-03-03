namespace Sodium
{
    internal class SwitchEvent<T> : Event<T>
    {
        private IEventListener<Event<T>> behaviorListener;
        private ISodiumCallback<T> wrappedEventListenerCallback;
        private IEventListener<T> wrappedEventListener;
        private Behavior<Event<T>> source;

        public SwitchEvent(Behavior<Event<T>> source)
        {
            this.source = source;
            this.Initialize();
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
            this.source = null;

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

                this.wrappedEventListener = newEvent.ListenSuppressed(this.wrappedEventListenerCallback, this.Rank, transaction);
            });
        }

        private void Initialize()
        {
            this.Run(this.Initialize);
        }

        private void Initialize(Transaction transaction)
        {
            this.wrappedEventListenerCallback = this.CreateFireCallback();
            this.wrappedEventListener = source.Sample().Listen(this.wrappedEventListenerCallback, this.Rank, transaction);

            var behaviorEventChanged = new ActionCallback<Event<T>>(UpdateWrappedEventListener);
            this.behaviorListener = source.Updates().Listen(behaviorEventChanged, this.Rank, transaction);
        }
    }
}
