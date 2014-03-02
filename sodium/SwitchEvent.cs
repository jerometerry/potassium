namespace Sodium
{
    internal class SwitchEvent<T> : Event<T>
    {
        private IEventListener<Event<T>> listener;
        private Event<Event<T>> updates;
        private Behavior<Event<T>> source;

        public SwitchEvent(Transaction transaction, Behavior<Event<T>> source)
        {
            this.source = source;
            var action = new SodiumAction<T>(this.Fire);
            var callback = new SwitchEventAction<T>(source, this, transaction, action);
            this.updates = source.Updates();
            this.listener = updates.Listen(transaction, callback, this.Rank);
        }

        public override void Dispose()
        {
            if (this.listener != null)
            {
                this.listener.Dispose();
                this.listener = null;
            }

            updates = null;
            this.source = null;

            base.Dispose();
        }
    }
}
