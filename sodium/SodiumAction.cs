namespace Sodium
{
    using System;

    internal sealed class SodiumAction<TA> : ISodiumAction<TA>
    {
        private readonly Action<Transaction, TA> action;

        public SodiumAction(Action<Transaction, TA> action)
        {
            this.action = action;
        }

        public void Invoke(Transaction transaction, TA data)
        {
            action(transaction, data);
        }
    }
}