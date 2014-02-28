namespace Sodium
{
    using System;

    internal sealed class CoalesceEvent<TA> : Event<TA>
    {
        private Event<TA> evt;
        private Func<TA, TA, TA> coalesce;
        private IEventListener<TA> listener;
        private Maybe<TA> accumulatedValue = Maybe<TA>.Null;

        public CoalesceEvent(Event<TA> evt, Func<TA, TA, TA> coalesce, Transaction transaction)
        {
            this.evt = evt;
            this.coalesce = coalesce;

            var action = new SodiumAction<TA>(this.Accumulate);
            this.listener = evt.Listen(transaction, action, this.Rank);
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

        public override void Close()
        {
            if (listener != null)
            {
                listener.Close();
                listener = null;
            }

            if (evt != null)
            {
                evt = null;
            }

            coalesce = null;

            base.Close();
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