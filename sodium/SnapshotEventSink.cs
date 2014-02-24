namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class SnapshotEventSink<TA, TB, TC> : EventSink<TC>
    {
        private readonly Event<TA> evt;
        private readonly Func<TA, TB, TC> snapshot;
        private readonly Behavior<TB> behavior;

        public SnapshotEventSink(Event<TA> ev, Func<TA, TB, TC> snapshot, Behavior<TB> behavior)
        {
            this.evt = ev;
            this.snapshot = snapshot;
            this.behavior = behavior;
        }

        public void SnapshotAndSend(Transaction transaction, TA firing)
        {
            this.Send(transaction, this.snapshot(firing, this.behavior.Sample()));
        }

        protected internal override TC[] SampleNow()
        {
            var events = evt.SampleNow();
            if (events == null)
            {
                return null;
            }
            
            var results = events.Select(e => this.snapshot(e, this.behavior.Sample()));
            return results.ToArray();
        }
    }
}