namespace Potassium.Internal
{
    using System;
    using System.Linq;
    using Potassium.Core;

    internal class FilteredEvent<T> : FireEvent<T>
    {
        private Observable<T> source;
        private Func<T, bool> f;
        private ISubscription<T> subscription;

        public FilteredEvent(Observable<T> source, Func<T, bool> f)
        {
            this.source = source;
            this.f = f;

            var forward = this.CreateRepeatObserver();
            this.subscription = source.Subscribe(forward, this.Priority);
        }

        public override T[] SubscriptionFirings()
        {
            var events = GetSubscribeFirings(this.source);
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

        /// <summary>
        /// Fire the event if the predicate evaluates to true
        /// </summary>
        /// <param name="t"></param>
        /// <param name="value"></param>
        internal override bool Fire(T value, Transaction t)
        {
            return this.f(value) && base.Fire(value, t);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            this.source = null;
            f = null;

            base.Dispose(disposing);
        }
    }
}