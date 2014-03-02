namespace Sodium
{
    internal sealed class EventListener<T> : SodiumObject, IEventListener<T>
    {
        public EventListener(Event<T> source, ISodiumAction<T> action, Rank rank)
        {
            this.Source = source;
            this.Action = action;
            this.Rank = rank;
        }

        public ISodiumAction<T> Action { get; private set; }

        public Rank Rank { get; private set; }

        public Event<T> Source { get; private set; }

        public override void Dispose()
        {
            if (this.Source != null)
            {
                this.Source.RemoveListener(this);
                this.Source = null;
            }

            Action = null;
            Rank = null;

            base.Dispose();
        }
    }
}