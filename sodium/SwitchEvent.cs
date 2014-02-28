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

        public override void Close()
        {
            this.listener = null;
            eventSwitchCallback = null;
            updates = null;
            this.behavior = null;

            base.Close();
        }
    }
}
