namespace Sodium
{
    internal sealed class Switch<T> : EventSink<T>
    {
        private ISubscription<IEvent<T>> behaviorSubscription;
        private ISodiumCallback<T> wrappedEventSubscriptionCallback;
        private ISubscription<T> wrappedSubscription;
        private IValue<IEvent<T>> source;

        public Switch(IValue<IEvent<T>> source)
        {
            this.source = source;
            this.StartTransaction(this.Initialize);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.behaviorSubscription != null)
            {
                this.behaviorSubscription.Dispose();
                this.behaviorSubscription = null;
            }

            if (this.wrappedSubscription != null)
            {
                this.wrappedSubscription.Dispose();
                this.wrappedSubscription = null;
            }

            this.wrappedEventSubscriptionCallback = null;
            this.source = null;

            base.Dispose(disposing);
        }

        private Unit Initialize(Transaction transaction)
        {
            this.wrappedEventSubscriptionCallback = this.CreateFireCallback();
            this.wrappedSubscription = source.Value.Subscribe(this.wrappedEventSubscriptionCallback, this.Rank, transaction);

            var behaviorEventChanged = new SodiumCallback<IEvent<T>>(this.UpdateWrappedEventSubscription);
            this.behaviorSubscription = source.Subscribe(behaviorEventChanged, this.Rank, transaction);

            return Unit.Nothing;
        }

        private void UpdateWrappedEventSubscription(IEvent<T> newEvent, Transaction transaction)
        {
            transaction.Medium(() =>
            {
                if (this.wrappedSubscription != null)
                {
                    this.wrappedSubscription.Dispose();
                    this.wrappedSubscription = null;
                }

                var suppressed = new SuppressedSubscribe<T>(newEvent);
                this.wrappedSubscription = suppressed.Subscribe(this.wrappedEventSubscriptionCallback, this.Rank, transaction);
                ((DisposableObject)this.wrappedSubscription).Register(suppressed);
            });
        }
    }
}
