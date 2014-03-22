namespace JT.Rx.Net
{
    
    using System.Linq;

    internal sealed class MergeEvent<T> : SubscribePublishEvent<T>
    {
        private Observable<T> source1;
        private Observable<T> source2;
        private ISubscription<T> l1;
        private ISubscription<T> l2;

        public MergeEvent(Observable<T> source1, Observable<T> source2)
        {
            this.source1 = source1;
            this.source2 = source2;

            var callback = this.CreateSubscriptionPublisher();
            l1 = source1.Subscribe(callback, this.Priority);
            l2 = source2.Subscribe(callback, this.Priority);
        }

        public override T[] SubscriptionFirings()
        {
            var firings1 = GetSubscribeFirings(source1);
            var firings2 = GetSubscribeFirings(source2);

            if (firings1 != null && firings2 != null)
            {
                return firings1.Concat(firings2).ToArray();
            }

            return firings1 ?? firings2;
        }

        protected override void Dispose(bool disposing)
        {
            if (l1 != null)
            {
                l1.Dispose();
                l1 = null;
            }

            if (l2 != null)
            {
                l2.Dispose();
                l2 = null;
            }

            source1 = null;
            source2 = null;

            base.Dispose(disposing);
        }
    }
}