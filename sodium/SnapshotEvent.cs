namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class SnapshotEvent<TA, TB, TC> : Event<TC>
    {
        private Event<TA> source;
        private Func<TA, TB, TC> snapshot;
        private Behavior<TB> behavior;
        private IEventListener<TA> listener;

        public SnapshotEvent(Event<TA> source, Func<TA, TB, TC> snapshot, Behavior<TB> behavior)
        {
            this.source = source;
            this.snapshot = snapshot;
            this.behavior = behavior;

            var action = new SodiumAction<TA>(this.Fire);
            this.listener = source.Listen(action, this.Rank);
        }

        public void Fire(Transaction transaction, TA firing)
        {
            this.Fire(transaction, this.snapshot(firing, this.behavior.Sample()));
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener = null;
            }

            if (source != null)
            {
                source = null;
            }

            if (behavior != null)
            {
                behavior = null;
            }

            snapshot = null;

            base.Dispose();
        }

        protected internal override TC[] InitialFirings()
        {
            var events = source.InitialFirings();
            if (events == null)
            {
                return null;
            }
            
            var results = events.Select(e => this.snapshot(e, this.behavior.Sample()));
            return results.ToArray();
        }
    }
}