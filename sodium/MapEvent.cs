namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MapEvent<T, TB> : SubscribePublishEvent<TB>
    {
        private IObservable<T> source;
        private Func<T, TB> map;
        private ISubscription<T> subscription;

        public MapEvent(IObservable<T> source, Func<T, TB> map)
        {
            this.source = source;
            this.map = map;
            this.subscription = source.Subscribe(new Notification<T>(this.Publish), this.Rank);
        }

        public void Publish(T publishing, Transaction trans)
        {
            var v = this.map(publishing);
            this.Publish(v, trans);
        }

        public override TB[] SubscriptionFirings()
        {
            var publishings = GetSubscribeFirings(source);
            if (publishings == null)
            { 
                return null;
            }

            return publishings.Select(e => map(e)).ToArray();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            source = null;
            map = null;

            base.Dispose(disposing);
        }
    }
}
