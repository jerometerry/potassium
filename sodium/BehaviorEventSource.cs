namespace Sodium
{
    using System;

    internal class BehaviorEventSource<T> : DisposableObject, IBehaviorSource<T>
    {
        private IEvent<T> source;

        private BehaviorEventSource(IEvent<T> source)
        {
            this.source = source;
        }

        public IEvent<T> Event
        {
            get { return source; }
        }

        public static BehaviorEventSource<T> ConstantEventSource()
        {
            return new BehaviorEventSource<T>(new Event<T>());
        }

        public static BehaviorEventSource<T> EventSinkSource()
        {
            return new BehaviorEventSource<T>(new EventSink<T>());
        }

        public static BehaviorEventSource<T> EventLoopSource()
        {
            return new BehaviorEventSource<T>(new EventLoop<T>());
        }

        public static BehaviorEventSource<T> Create(Event<T> source)
        {
            return new BehaviorEventSource<T>(source);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.source != null)
                {
                    this.source.Dispose();
                    this.source = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
