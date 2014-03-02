namespace Sodium
{
    internal class SwitchBehaviorEvent<T> : Event<T>
    {
        private IEventListener<Behavior<T>> listener;
        private IEventListener<T> eventListener;
        private Behavior<Behavior<T>> source;
        private Event<T> wrappedEvent;
        private Event<Behavior<T>> sourceEvent;

        public SwitchBehaviorEvent(Behavior<Behavior<T>> source)
        {
            this.source = source;
            this.sourceEvent = source.Value();
            var action = new SodiumCallback<Behavior<T>>(this.Invoke);
            this.listener = this.sourceEvent.Listen(action, this.Rank);
        }

        public void Invoke(Transaction transaction, Behavior<T> behavior)
        {
            // Note: If any switch takes place during a transaction, then the
            // Value().Listen will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using Value().Listen, and Value() throws away all firings except
            // for the last one. Therefore, anything from the old input behavior
            // that might have happened during this transaction will be suppressed.
            if (this.eventListener != null)
            {
                this.eventListener.Dispose();
                this.eventListener = null;
            }

            if (this.wrappedEvent != null)
            {
                this.wrappedEvent.Dispose();
                this.wrappedEvent = null;
            }

            this.wrappedEvent = behavior.Value(transaction);
            this.eventListener = wrappedEvent.Listen(transaction, new SodiumCallback<T>(Fire), Rank);
        }

        public override void Dispose()
        {
            if (this.listener != null)
            {
                this.listener.Dispose();
                this.listener = null;
            }

            if (this.eventListener != null)
            {
                this.eventListener.Dispose();
                this.eventListener = null;
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

            base.Dispose();
        }
    }
}
