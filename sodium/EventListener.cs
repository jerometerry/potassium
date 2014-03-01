namespace Sodium
{
    internal sealed class EventListener<TA> : IEventListener<TA>
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

        public void Dispose()
        {
            if (this.Event != null)
            {
                this.Event.RemoveListener(this);
                this.Event = null;
            }

            Action = null;
            Rank = null;
        }
    }
}