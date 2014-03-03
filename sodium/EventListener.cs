namespace Sodium
{
    internal sealed class EventListener<T> : SodiumObject, IEventListener<T>
    {
        private ISodiumCallback<T> callback;

        public EventListener(Event<T> source, ISodiumCallback<T> callback, Rank rank)
        {
            this.Source = source;
            this.callback = callback;
            this.Rank = rank;
        }

        public Rank Rank { get; private set; }

        public Event<T> Source { get; private set; }

        public void Fire(T firing, Transaction transaction)
        {
            if (this.callback != null)
            {
                this.callback.Fire(firing, transaction);
            }
        }

        public override void Dispose()
        {
            if (this.Source != null)
            {
                this.Source.RemoveListener(this);
                this.Source = null;
            }

            callback = null;
            Rank = null;

            base.Dispose();
        }
    }
}