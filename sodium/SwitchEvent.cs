namespace Sodium
{
    internal class SwitchEvent<TA> : Event<TA>
    {
        private SwitchEventAction<TA> eventSwitchCallback;
        private IEventListener<Event<TA>> listener;
        private Event<Event<TA>> updates;
        private Behavior<Event<TA>> behavior;

        public SwitchEvent(Transaction transaction, Behavior<Event<TA>> behavior)
        {
            this.behavior = behavior;
            var action = new SodiumAction<TA>(this.Fire);
            this.eventSwitchCallback = new SwitchEventAction<TA>(behavior, this, transaction, action);
            this.updates = behavior.Updates();
            this.listener = updates.Listen(transaction, eventSwitchCallback, this.Rank);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.listener != null)
                {
                    this.listener.Dispose();
                    this.listener = null;
                }

                if (eventSwitchCallback != null)
                {
                    eventSwitchCallback.Dispose();
                    eventSwitchCallback = null;
                }

                if (updates != null)
                {
                    updates.Dispose();
                    updates = null;
                }

                if (this.behavior != null)
                {
                    this.behavior.Dispose();
                    this.behavior = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
