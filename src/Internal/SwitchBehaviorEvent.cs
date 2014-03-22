namespace JT.Rx.Net.Internal
{
    using JT.Rx.Net.Core;    

    internal sealed class SwitchBehaviorEvent<T> : EventPublisher<T>
    {
        private ISubscription<Behavior<T>> subscription;
        private ISubscription<T> wrappedSubscription;
        private Event<T> wrappedEvent;

        public SwitchBehaviorEvent(Behavior<Behavior<T>> source)
        {
            var evt = source.Values();
            var callback = new SubscriptionPublisher<Behavior<T>>(this.Invoke);
            this.subscription = evt.Subscribe(callback, this.Priority);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            if (this.wrappedSubscription != null)
            {
                this.wrappedSubscription.Dispose();
                this.wrappedSubscription = null;
            }

            if (this.wrappedEvent != null)
            {
                this.wrappedEvent.Dispose();
                this.wrappedEvent = null;
            }

            base.Dispose(disposing);
        }

        private void Invoke(Behavior<T> behavior, Transaction transaction)
        {
            // Note: If any switch takes place during a transaction, then the
            // GetValueStream().Subscribe will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using GetValueStream().Subscribe, and GetValueStream() throws away all publishings except
            // for the last one. Therefore, anything from the old input behavior
            // that might have happened during this transaction will be suppressed.
            if (this.wrappedSubscription != null)
            {
                this.wrappedSubscription.Dispose();
                this.wrappedSubscription = null;
            }

            if (this.wrappedEvent != null)
            {
                this.wrappedEvent.Dispose();
                this.wrappedEvent = null;
            }

            this.wrappedEvent = new BehaviorLastValueEvent<T>(behavior, transaction);
            this.wrappedSubscription = this.wrappedEvent.CreateSubscription(this.CreateSubscriptionPublisher(), this.Priority, transaction);
        }
    }
}