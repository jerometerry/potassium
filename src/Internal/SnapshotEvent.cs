namespace Potassium.Internal
{
    using System;
    using System.Linq;
    using Potassium.Core;
    using Potassium.Providers;

    /// <summary>
    /// SnapshotEvent is an event that publishes snapshots of a source event, computed by
    /// invoking a snapshot function with the current published value of the source event
    /// as the first parameter, and the current value of the IProvider as the second parameter.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter of the snapshot function</typeparam>
    /// <typeparam name="TB">The type of the second parameter of the snapshot function</typeparam>
    /// <typeparam name="TS">The return type of the snapshot function</typeparam>
    internal sealed class SnapshotEvent<T, TB, TS> : PublishEvent<TS>
    {
        private Observable<T> source;
        private Func<T, TB, TS> snapshot;
        private IProvider<TB> provider;
        private ISubscription<T> subscription;

        public SnapshotEvent(Observable<T> source, Func<T, TB, TS> snapshot, IProvider<TB> provider)
        {
            this.source = source;
            this.snapshot = snapshot;
            this.provider = provider;

            var callback = new SubscriptionPublisher<T>(this.PublishSnapshot);
            this.subscription = source.Subscribe(callback, this.Priority);
        }

        public override TS[] SubscriptionFirings()
        {
            var events = GetSubscribeFirings(source);
            if (events == null)
            {
                return null;
            }
            
            var results = events.Select(e => this.snapshot(e, this.provider.Value));
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
            this.provider = null;
            snapshot = null;

            base.Dispose(disposing);
        }

        private void PublishSnapshot(T value, Transaction transaction)
        {
            var v = snapshot(value, provider.Value);
            this.Publish(v, transaction);
        }
    }
}