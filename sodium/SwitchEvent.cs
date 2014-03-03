namespace Sodium
{
    internal class SwitchEvent<T> : EventSink<T>
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

        protected override void Dispose(bool disposing)
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

            base.Dispose(disposing);
        }

        private void Initialize()
        {
            this.Run(this.Initialize);
        }

        private void Initialize(Transaction transaction)
        {
            this.wrappedEventListenerCallback = this.CreateFireCallback();
            this.wrappedEventListener = source.Value.Listen(this.wrappedEventListenerCallback, this.Rank, transaction);

            var behaviorEventChanged = new ActionCallback<Event<T>>(UpdateWrappedEventListener);
            this.behaviorListener = source.Listen(behaviorEventChanged, this.Rank, transaction);
        }

        private void UpdateWrappedEventListener(Event<T> newEvent, Transaction transaction)
        {
            transaction.Last(() =>
            {
                if (this.wrappedEventListener != null)
                {
                    this.wrappedEventListener.Dispose();
                    this.wrappedEventListener = null;
                }

                var suppressed = new SuppressedListenEvent<T>(newEvent);
                this.wrappedEventListener = suppressed.Listen(this.wrappedEventListenerCallback, this.Rank, transaction);
                ((SodiumObject)this.wrappedEventListener).RegisterFinalizer(suppressed);
            });
        }
    }
}
