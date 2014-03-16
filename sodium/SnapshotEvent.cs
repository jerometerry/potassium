namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class SnapshotEvent<T, TB, TC> : InitialFireEventSink<TC>
    {
        private Event<T> source;
        private Func<T, TB, TC> snapshot;
        private Behavior<TB> behavior;
        private ISubscription<T> subscription;

        public SnapshotEvent(Event<T> source, Func<T, TB, TC> snapshot, Behavior<TB> behavior)
        {
            this.source = source;
            this.snapshot = snapshot;
            this.behavior = behavior;

            var callback = new ActionCallback<T>(this.Fire);
            this.subscription = source.Subscribe(callback, this.Rank);
        }

        public void Fire(T firing, Transaction transaction)
        {
            var f = this.behavior.Value;
            var v = this.snapshot(firing, f);
            this.Fire(v, transaction);
        }

        protected internal override TC[] InitialFirings()
        {
            var events = GetInitialFirings(source);
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