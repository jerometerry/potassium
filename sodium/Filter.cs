namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class Filter<T> : SubscribeFireEvent<T>
    {
        private IObservable<T> source;
        private Func<T, bool> f;
        private ISubscription<T> subscription;

        public Filter(IObservable<T> source, Func<T, bool> f)
        {
            this.source = source;
            this.f = f;

            var callback = this.CreateFireCallback();
            this.subscription = source.Subscribe(callback, this.Rank);
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
        /// <param name="a"></param>
        protected override bool Fire(T a, Transaction t)
        {
            return this.f(a) && base.Fire(a, t);
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