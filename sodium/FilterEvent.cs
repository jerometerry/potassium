namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class FilterEvent<TA> : Event<TA>
    {
        private Event<TA> evt;
        private Func<TA, bool> f;
        private IEventListener<TA> listener;

        public FilterEvent(Event<TA> evt, Func<TA, bool> f)
        {
            this.evt = evt;
            this.f = f;

            var action = new SodiumAction<TA>(this.Fire);
            this.listener = evt.Listen(action, this.Rank);
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            if (evt != null)
            {
                evt = null;
            }

            f = null;

            base.Dispose();
        }

        /// <summary>
        /// Fire the event if the predicate evaluates to true
        /// </summary>
        /// <param name="t"></param>
        /// <param name="a"></param>
        internal override void Fire(Transaction t, TA a)
        {
            if (f(a))
            {
                base.Fire(t, a);
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