namespace Sodium
{
    using System;

    internal class EventSource<T> : IBehaviorSource<T>
    {
        Event<T> source;

        public Event<T> Event
        {
            get { return source; }
        }

        private EventSource(Event<T> source)
        {
            this.source = source;
        }

        public static EventSource<T> ConstantEventSource()
        {
            return new EventSource<T>(new Event<T>());
        }

        public static EventSource<T> EventSinkSource()
        {
            return new EventSource<T>(new EventSink<T>());
        }

        public static EventSource<T> EventLoopSource()
        {
            return new EventSource<T>(new EventLoop<T>());
        }

        public static EventSource<T> Create(Event<T> source)
        {
            return new EventSource<T>(source);
        }

        public ISnapshot<T> Coalesce(Func<T, T, T> coalesce)
        {
            return this.source.Coalesce(coalesce);
        }

        public IHoldable<TB> Map<TB>(Func<T, TB> map)
        {
            return this.source.Map(map);
        }

        public bool CancelSubscription(ISubscription<T> subscription)
        {
            return this.source.CancelSubscription(subscription);
        }

        public ISubscription<T> Subscribe(Action<T> callback)
        {
            return this.source.Subscribe(callback);
        }

        public ISubscription<T> Subscribe(ISodiumCallback<T> callback)
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
