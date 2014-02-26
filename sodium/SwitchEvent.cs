namespace Sodium
{
    internal class SwitchEvent<TA> : Event<TA>
    {
        public SwitchEvent(Transaction transaction, Behavior<Event<TA>> behavior)
        {
            var callback = new Callback<TA>(this.Fire);
            var eventSwitchCallback = new SwitchEventCallback<TA>(behavior, this, transaction, callback);
            var listener = behavior.Updates().Listen(transaction, eventSwitchCallback, this.Rank);
            this.RegisterListener(listener);
        }
    }
}
