namespace Sodium
{
    using System;

    internal sealed class Trigger<TA> : ITrigger<TA>
    {
        private readonly Action<Transaction, TA> action;

        public Trigger(Action<Transaction, TA> action)
        {
            this.action = action;
        }

        public void Fire(Transaction trans, TA a)
        {
            action(trans, a);
        }
    }
}