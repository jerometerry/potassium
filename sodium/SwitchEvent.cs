namespace Sodium
{
    internal class SwitchEvent<TA> : Event<TA>
    {
        private SwitchEventCallback<TA> eventSwitchCallback;

        public SwitchEvent(Transaction transaction, Behavior<Event<TA>> behavior)
        {
            var callback = new Callback<TA>(this.Fire);
            eventSwitchCallback = new SwitchEventCallback<TA>(behavior, this, transaction, callback);
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
