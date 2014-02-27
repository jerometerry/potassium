namespace Sodium
{
    internal class SwitchEvent<TA> : Event<TA>
    {
        private SwitchEventAction<TA> eventSwitchCallback;
        private IEventListener<Event<TA>> listener;

        public SwitchEvent(Transaction transaction, Behavior<Event<TA>> behavior)
        {
            var action = new SodiumAction<TA>(this.Fire);
            eventSwitchCallback = new SwitchEventAction<TA>(behavior, this, transaction, action);
            this.listener = behavior.Updates().Listen(transaction, eventSwitchCallback, this.Rank);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (eventSwitchCallback != null)
                {
                    eventSwitchCallback.Dispose();
                    eventSwitchCallback = null;
                }

                if (this.listener != null)
                {
                    this.listener.Dispose();
                    this.listener = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
