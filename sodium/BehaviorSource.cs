namespace Sodium
{
    using System;

    internal class BehaviorSource<T> : IBehaviorSource<T>
    {
        Event<T> source;

        public Event<T> Event
        {
            get { return source; }
            private set { source = value; }
        }

        public BehaviorSource(Event<T> source)
        {
            this.source = source;
        }

        public ISnapshot<T> Coalesce(Func<T, T, T> coalesce)
        {
            return this.source.Coalesce(coalesce);
        }

        public IHoldable<TB> Map<TB>(Func<T, TB> map)
        {
            return this.source.Map(map);
        }

        public ISubscription<T> Subscribe(Action<T> callback)
        {
            return this.source.Subscribe(callback);
        }

        public ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank rank)
        {
            return this.source.Subscribe(callback, rank);
        }

        public ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank rank, Transaction transaction)
        {
            return this.source.Subscribe(callback, rank, transaction);
        }

        public void Dispose()
        {
            if (this.source != null)
            {
                this.source.Dispose();
                this.source = null;
            }
        }
    }
}
