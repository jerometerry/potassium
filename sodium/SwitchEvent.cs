namespace Sodium
{
    internal class SwitchEvent<TA> : Event<TA>
    {
        private SwitchEventAction<TA> eventSwitchCallback;

        public SwitchEvent(Transaction transaction, Behavior<Event<TA>> behavior)
        {
            var action = new SodiumAction<TA>(this.Fire);
            eventSwitchCallback = new SwitchEventAction<TA>(behavior, this, transaction, action);
            behavior.Updates().Listen(transaction, eventSwitchCallback, this.Rank);
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
            }

            base.Dispose(disposing);
        }
    }
}
