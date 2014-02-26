namespace Sodium
{
    internal class SwitchEvent<TA> : Event<TA>
    {
        IListener listener;

        public SwitchEvent(Transaction transaction, Behavior<Event<TA>> behavior)
        {
            var callback = new Callback<TA>(this.Fire);
            var eventSwitchCallback = new SwitchEventCallback<TA>(behavior, this, transaction, callback);
            listener = behavior.Updates().Listen(transaction, eventSwitchCallback, this.Rank);
        }
    }
}
