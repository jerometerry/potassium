namespace Sodium
{
    using System;

    internal class CoalesceEvent<T> : EventSink<T>
    {
        private Event<T> source;
        private Func<T, T, T> coalesce;
        private IEventListener<T> listener;
        private Maybe<T> accumulatedValue = Maybe<T>.Null;

        public CoalesceEvent(Event<T> source, Func<T, T, T> coalesce, Scheduler scheduler)
        {
            this.source = source;
            this.coalesce = coalesce;

            var callback = new ActionCallback<T>(this.Accumulate);
            this.listener = source.Listen(callback, this.Rank, scheduler);
        }

        protected internal override T[] InitialFirings()
        {
            var events = GetInitialFirings(source);
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

        protected override void Dispose(bool disposing)
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            source = null;
            coalesce = null;

            base.Dispose(disposing);
        }

        private void Accumulate(T data, Scheduler scheduler)
        {
            if (this.accumulatedValue.HasValue)
            {
                this.accumulatedValue = new Maybe<T>(coalesce(this.accumulatedValue.Value(), data));
            }
            else
            {
                scheduler.High(Fire, this.Rank);
                this.accumulatedValue = new Maybe<T>(data);
            }
        }

        private void Fire(Scheduler scheduler)
        {
            var v = this.accumulatedValue.Value();
            this.Fire(v, scheduler);
            this.accumulatedValue = Maybe<T>.Null;
        }
    }
}