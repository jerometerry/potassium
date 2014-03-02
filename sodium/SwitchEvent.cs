namespace Sodium
{
    internal class SwitchEvent<T> : Event<T>
    {
        private SwitchEventAction<T> eventSwitchCallback;
        private IEventListener<Event<T>> listener;
        private Event<Event<T>> updates;
        private Behavior<Event<T>> source;

        public SwitchEvent(Transaction transaction, Behavior<Event<T>> source)
        {
            this.source = source;
            var action = new SodiumAction<T>(this.Fire);
            this.eventSwitchCallback = new SwitchEventAction<T>(source, this, transaction, action);
            this.updates = source.Updates();
            this.listener = updates.Listen(transaction, eventSwitchCallback, this.Rank);
        }

        public override void Dispose()
        {
            if (this.listener != null)
            {
                this.listener.Dispose();
                this.listener = null;
            }

            if (this.eventSwitchCallback != null)
            {
                eventSwitchCallback = null;
            }
            
            updates = null;
            this.source = null;

            base.Dispose();
        }
    }
}
