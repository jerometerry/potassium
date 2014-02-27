namespace Sodium
{
    using System;

    internal sealed class CoalesceAction<TA> : ISodiumAction<TA>
    {
        private readonly Func<TA, TA, TA> coalesce;
        private readonly Event<TA> evt;
        private Maybe<TA> accum = Maybe<TA>.Null;

        public CoalesceAction(Event<TA> evt, Func<TA, TA, TA> coalesce)
        {
            this.evt = evt;
            this.coalesce = coalesce;
        }

        public void Invoke(Transaction transaction, TA data)
        {
            if (accum.HasValue)
            {
                accum = new Maybe<TA>(coalesce(accum.Value(), data));
            }
            else
            {
                transaction.Prioritize(Fire, evt.Rank);
                accum = new Maybe<TA>(data);
            }
        }

        private void Fire(Transaction transaction)
        {
            evt.Fire(transaction, accum.Value());
            accum = Maybe<TA>.Null;
        }
    }
}