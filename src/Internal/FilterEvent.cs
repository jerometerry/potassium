namespace Potassium.Internal
{
    using System;
    using System.Linq;
    using Potassium.Core;

    internal class FilterEvent<T> : PublishEvent<T>
    {
        private Observable<T> source;
        private Func<T, bool> f;
        private ISubscription<T> subscription;

        public FilterEvent(Observable<T> source, Func<T, bool> f)
        {
            this.source = source;
            this.f = f;

            var callback = this.CreateSubscriptionPublisher();
            this.subscription = source.Subscribe(callback, this.Priority);
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
        /// Publish the event if the predicate evaluates to true
        /// </summary>
        /// <param name="t"></param>
        /// <param name="value"></param>
        internal override bool Publish(T value, Transaction t)
        {
            return this.f(value) && base.Publish(value, t);
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