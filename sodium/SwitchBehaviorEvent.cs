namespace Sodium
{
    internal class SwitchBehaviorEvent<T> : EventSink<T>
    {
        private ISubscription<Behavior<T>> subscription;
        private ISubscription<T> wrappedSubscription;
        private Behavior<Behavior<T>> source;
        private Event<T> wrappedEvent;
        private Event<Behavior<T>> sourceEvent;

        public SwitchBehaviorEvent(Behavior<Behavior<T>> source)
        {
            this.source = source;
            this.sourceEvent = source.Values();
            var callback = new ActionCallback<Behavior<T>>(this.Invoke);
            this.subscription = this.sourceEvent.Subscribe(callback, this.Rank);
        }

        public void Invoke(Behavior<T> behavior, Transaction transaction)
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

            this.wrappedEvent = behavior.Values(transaction);
            this.wrappedSubscription = wrappedEvent.Subscribe(this.CreateFireCallback(), Rank, transaction);
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

            if (this.sourceEvent != null)
            {
                this.sourceEvent.Dispose();
                this.sourceEvent = null;
            }

            this.source = null;

            base.Dispose(disposing);
        }
    }
}
