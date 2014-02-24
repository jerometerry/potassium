namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class FilterEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> evt;
        private readonly Func<TA, bool> f;

        public FilterEventSink(Event<TA> evt, Func<TA, bool> f)
        {
            this.evt = evt;
            this.f = f;
        }

        /// <summary>
        /// Fire the event if the predicate evaluates to true
        /// </summary>
        /// <param name="t"></param>
        /// <param name="a"></param>
        public void FireIfNotFiltered(Transaction t, TA a)
        {
            if (f(a))
            {
                Fire(t, a);
            }
        }

        protected internal override TA[] InitialFirings()
        {
            var events = evt.InitialFirings();
            if (events == null)
            {
                return null;
            }

            var filtered = events.Where(e => f(e)).ToList();
            if (!filtered.Any())
            {
                return null;
            }

            return filtered.ToArray();
        }
    }
}