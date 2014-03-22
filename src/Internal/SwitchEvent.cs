namespace JT.Rx.Net.Internal
{
    using JT.Rx.Net.Core;

    internal sealed class SwitchEvent<T> : EventPublisher<T>
    {
        private ISubscription<Event<T>> behaviorSubscription;
        private SubscriptionPublisher<T> wrappedEventSubscriptionCallback;
        private ISubscription<T> wrappedSubscription;
        private Behavior<Event<T>> source;

        public SwitchEvent(Behavior<Event<T>> source)
        {
            this.source = source;
            Transaction.Start(this.Initialize);
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
            this.wrappedEventSubscriptionCallback = this.CreateSubscriptionPublisher();
            this.wrappedSubscription = source.Value.CreateSubscription(this.wrappedEventSubscriptionCallback, this.Priority, transaction);

            var behaviorEventChanged = new SubscriptionPublisher<Event<T>>(this.UpdateWrappedEventSubscription);
            this.behaviorSubscription = source.Source.CreateSubscription(behaviorEventChanged, this.Priority, transaction);

            return Unit.Nothing;
        }

        private void UpdateWrappedEventSubscription(Event<T> newEvent, Transaction transaction)
        {
            transaction.Medium(() =>
            {
                if (this.wrappedSubscription != null)
                {
                    this.wrappedSubscription.Dispose();
                    this.wrappedSubscription = null;
                }

                var suppressed = new SuppressedSubscribeEvent<T>(newEvent);
                this.wrappedSubscription = suppressed.CreateSubscription(this.wrappedEventSubscriptionCallback, this.Priority, transaction);
                ((Disposable)this.wrappedSubscription).Register(suppressed);
            });
        }
    }
}
