namespace Sodium
{
    internal sealed class EventListener<TA> : SodiumItem, IEventListener<TA>
    {
        public EventListener(Event<TA> evt, ISodiumAction<TA> action, Rank rank, bool allowAutoDispose)
            : base(allowAutoDispose)
        {
            this.Event = evt;
            this.Action = action;
            this.Rank = rank;
        }

        public ISodiumAction<TA> Action { get; private set; }

        public Rank Rank { get; private set; }

        public Event<TA> Event { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
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
}