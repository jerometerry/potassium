namespace Sodium
{
    internal sealed class SwitchBehavior<T> : Sink<T>
    {
        private ISubscription<IBehavior<T>> subscription;
        private ISubscription<T> wrappedSubscription;
        private IEvent<T> wrappedEvent;

        public SwitchBehavior(IBehavior<IBehavior<T>> source)
        {
            var callback = new SodiumCallback<IBehavior<T>>(this.Invoke);
            this.subscription = source.SubscribeValues(callback, this.Rank);
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

        private void Invoke(IBehavior<T> behavior, Transaction transaction)
        {
            // Note: If any switch takes place during a transaction, then the
            // GetValueStream().Subscribe will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using GetValueStream().Subscribe, and GetValueStream() throws away all firings except
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

            this.wrappedEvent = new SubscribeFireLastValueEvent<T>(behavior, transaction);
            this.wrappedSubscription = this.wrappedEvent.Subscribe(this.CreateFireCallback(), this.Rank, transaction);
        }
    }
}