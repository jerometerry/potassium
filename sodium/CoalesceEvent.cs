namespace Sodium
{
    using System;

    internal sealed class CoalesceEvent<TA> : Event<TA>
    {
        private readonly Event<TA> evt;
        private readonly Func<TA, TA, TA> coalesce;

        public CoalesceEvent(Event<TA> evt, Func<TA, TA, TA> coalesce, Transaction transaction)
        {
            this.evt = evt;
            this.coalesce = coalesce;

            var callback = new CoalesceCallback<TA>(this, coalesce);
            var listener = evt.ListenUnsuppressed(transaction, callback, this.Rank);
            this.RegisterListener(listener);
        }

        protected internal override TA[] InitialFirings()
        {
            var events = evt.InitialFirings();
            if (events == null)
            {
                return null;
            }
            
            var e = events[0];
            for (var i = 1; i < events.Length; i++)
            {
                e = coalesce(e, events[i]);
            }

            return new[] { e };
        }
    }
}