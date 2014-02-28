namespace Sodium
{
    internal sealed class EventListener<TA> : SodiumItem, IEventListener<TA>
    {
        public EventListener(Event<TA> evt, ISodiumAction<TA> action, Rank rank)
        {
            this.Event = evt;
            this.Action = action;
            this.Rank = rank;
        }

        public ISodiumAction<TA> Action { get; private set; }

        public Rank Rank { get; private set; }

        public Event<TA> Event { get; private set; }

        public override void Close()
        {
            if (this.Event != null)
            {
                this.Event.RemoveListener(this);
                this.Event = null;
            }

            Action = null;
            Rank = null;

            base.Close();
        }
    }
}