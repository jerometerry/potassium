namespace Sodium
{
    using System;

    internal sealed class CoalesceEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> evt;
        private readonly Func<TA, TA, TA> coalesce;

        public CoalesceEventSink(Event<TA> evt, Func<TA, TA, TA> coalesce)
        {
            this.evt = evt;
            this.coalesce = coalesce;
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