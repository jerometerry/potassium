namespace Sodium
{
    internal sealed class EventListener<TA> : SodiumObject, IEventListener<TA>
    {
        public EventListener(Event<TA> source, ISodiumAction<TA> action, Rank rank)
        {
            this.Source = source;
            this.Action = action;
            this.Rank = rank;
        }

        public ISodiumAction<TA> Action { get; private set; }

        public Rank Rank { get; private set; }

        public Event<TA> Source { get; private set; }

        public void Dispose()
        {
            if (this.Source != null)
            {
                this.Source.RemoveListener(this);
                this.Source = null;
            }

            Action = null;
            Rank = null;
        }
    }
}