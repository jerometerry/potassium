namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class SnapshotEvent<T, TB, TC> : SubscribePublishEvent<TC>
    {
        private Observable<T> source;
        private Func<T, TB, TC> snapshot;
        private IBehavior<TB> behavior;
        private ISubscription<T> subscription;

        public SnapshotEvent(Observable<T> source, Func<T, TB, TC> snapshot, IBehavior<TB> behavior)
        {
            this.source = source;
            this.snapshot = snapshot;
            this.behavior = behavior;

            var callback = new SubscriptionPublisher<T>(this.Publish);
            this.subscription = source.Subscribe(callback, this.Priority);
        }

        public void Publish(T publishing, Transaction transaction)
        {
            var f = this.behavior.Value;
            var v = this.snapshot(publishing, f);
            this.Publish(v, transaction);
        }

        public override TC[] SubscriptionFirings()
        {
            var events = GetSubscribeFirings(source);
            if (events == null)
            {
                return null;
            }
            
            var results = events.Select(e => this.snapshot(e, this.behavior.Value));
            return results.ToArray();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            source = null;
            behavior = null;
            snapshot = null;

            base.Dispose(disposing);
        }
    }
}