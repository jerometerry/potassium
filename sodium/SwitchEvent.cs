namespace Sodium
{
    internal class SwitchEvent<TA> : Event<TA>
    {
        private SwitchEventAction<TA> eventSwitchCallback;
        private IEventListener<Event<TA>> listener;
        private Event<Event<TA>> updates;
        private Behavior<Event<TA>> source;

        public SwitchEvent(Transaction transaction, Behavior<Event<TA>> source)
        {
            this.source = source;
            var action = new SodiumAction<TA>(this.Fire);
            this.eventSwitchCallback = new SwitchEventAction<TA>(source, this, transaction, action);
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
