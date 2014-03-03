namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class FilterEvent<T> : EventSink<T>
    {
        private Event<T> source;
        private Func<T, bool> f;
        private IEventListener<T> listener;

        public FilterEvent(Event<T> source, Func<T, bool> f)
        {
            this.source = source;
            this.f = f;

            var callback = this.CreateFireCallback();
            this.listener = source.Listen(callback, this.Rank);
        }

        /// <summary>
        /// Fire the event if the predicate evaluates to true
        /// </summary>
        /// <param name="t"></param>
        /// <param name="a"></param>
        public override bool Fire(T a, Transaction t)
        {
            return this.f(a) && base.Fire(a, t);
        }

        protected internal override T[] InitialFirings()
        {
            var events = GetInitialFirings(this.source);
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

        protected override void Dispose(bool disposing)
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            this.source = null;
            f = null;

            base.Dispose(disposing);
        }
    }
}