namespace Sodium
{
    using System;

    internal sealed class CoalesceEvent<TA> : Event<TA>
    {
        private Event<TA> source;
        private Func<TA, TA, TA> coalesce;
        private IEventListener<TA> listener;
        private Maybe<TA> accumulatedValue = Maybe<TA>.Null;

        public CoalesceEvent(Event<TA> source, Func<TA, TA, TA> coalesce, Transaction transaction)
        {
            this.source = source;
            this.coalesce = coalesce;

            var action = new SodiumAction<TA>(this.Accumulate);
            this.listener = source.Listen(transaction, action, this.Rank);
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            if (source != null)
            {
                source = null;
            }

            coalesce = null;

            base.Dispose();
        }

        protected internal override TA[] InitialFirings()
        {
            var events = source.InitialFirings();
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

        private void Accumulate(Transaction transaction, TA data)
        {
            if (this.accumulatedValue.HasValue)
            {
                this.accumulatedValue = new Maybe<TA>(coalesce(this.accumulatedValue.Value(), data));
            }
            else
            {
                transaction.Priority(Fire, this.Rank);
                this.accumulatedValue = new Maybe<TA>(data);
            }
        }

        private void Fire(Transaction transaction)
        {
            this.Fire(transaction, this.accumulatedValue.Value());
            this.accumulatedValue = Maybe<TA>.Null;
        }
    }
}