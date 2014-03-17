namespace Sodium
{
    using System;
    using System.Collections.Generic;

    internal class CoalesceEvent<T> : InitialFireEventSink<T>
    {
        private IObservable<T> source;
        private Func<T, T, T> coalesce;
        private ISubscription<T> subscription;
        private Maybe<T> accumulatedValue = Maybe<T>.Null;

        public CoalesceEvent(IObservable<T> source, Func<T, T, T> coalesce, Transaction transaction)
        {
            this.source = source;
            this.coalesce = coalesce;

            var callback = new ActionCallback<T>(this.Accumulate);
            this.subscription = source.Subscribe(callback, this.Rank, transaction);
        }

        public override T[] InitialFirings()
        {
            var events = GetInitialFirings(source);
            if (events == null)
            {
                return null;
            }
            
            var e = this.Coalesce(events);
            return new[] { e };
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            source = null;
            coalesce = null;

            base.Dispose(disposing);
        }

        private T Coalesce(IList<T> events)
        {
            var e = events[0];
            for (var i = 1; i < events.Count; i++)
            {
                e = this.coalesce(e, events[i]);
            }

            return e;
        }

        private void Accumulate(T data, Transaction transaction)
        {
            if (this.accumulatedValue.HasValue)
            {
                this.accumulatedValue = new Maybe<T>(coalesce(this.accumulatedValue.Value(), data));
            }
            else
            {
                this.ScheduleFiring(transaction);
                this.accumulatedValue = new Maybe<T>(data);
            }
        }

        private void ScheduleFiring(Transaction transaction)
        {
            transaction.High(this.Fire, this.Rank);
        }

        private void Fire(Transaction transaction)
        {
            var v = this.accumulatedValue.Value();
            this.Fire(v, transaction);
            this.accumulatedValue = Maybe<T>.Null;
        }
    }
}