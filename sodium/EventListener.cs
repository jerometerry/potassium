namespace Sodium
{
    public sealed class EventListener<T> : SodiumObject, IEventListener<T>
    {
        public EventListener(Event<T> source, ISodiumCallback<T> callback, Rank rank)
        {
            this.Source = source;
            this.Callback = callback;
            this.Rank = rank;
        }

        public Event<T> Source { get; private set; }

        public ISodiumCallback<T> Callback { get; private set; }

        public Rank Rank { get; private set; }

        public void Fire(T firing, Transaction transaction)
        {
            if (this.Callback != null)
            {
                this.Callback.Fire(firing, transaction);
            }
        }

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