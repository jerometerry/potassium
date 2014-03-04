namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class SnapshotEvent<T, TB, TC> : EventSink<TC>
    {
        private Event<T> source;
        private Func<T, TB, TC> snapshot;
        private Behavior<TB> behavior;
        private IEventListener<T> listener;

        public SnapshotEvent(Event<T> source, Func<T, TB, TC> snapshot, Behavior<TB> behavior)
        {
            this.source = source;
            this.snapshot = snapshot;
            this.behavior = behavior;

            var callback = new ActionCallback<T>(this.Fire);
            this.listener = source.Listen(callback, this.Rank);
        }

        public void Fire(T firing, Scheduler scheduler)
        {
            var f = this.behavior.Value;
            var v = this.snapshot(firing, f);
            this.Fire(v, scheduler);
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
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            source = null;
            behavior = null;
            snapshot = null;

            base.Dispose(disposing);
        }
    }
}