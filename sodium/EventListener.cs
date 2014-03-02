namespace Sodium
{
    internal sealed class EventListener<T> : SodiumObject, IEventListener<T>
    {
        public EventListener(Event<T> source, ISodiumCallback<T> callback, Rank rank)
        {
            this.Source = source;
            this.Callback = callback;
            this.Rank = rank;
        }

        public ISodiumCallback<T> Callback { get; private set; }

        public Rank Rank { get; private set; }

        public Event<T> Source { get; private set; }

        public override void Dispose()
        {
            if (this.Source != null)
            {
                this.Source.RemoveListener(this);
                this.Source = null;
            }

            Callback = null;
            Rank = null;

            base.Dispose();
        }
    }
}