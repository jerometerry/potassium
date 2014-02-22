namespace Sodium
{
    using System;

    internal sealed class TransactionHandler<TA> : ITransactionHandler<TA>
    {
        private readonly Action<Transaction, TA> action;

        public TransactionHandler(Action<Transaction, TA> action)
        {
            this.action = action;
        }

        public void Run(Transaction trans, TA a)
        {
            action(trans, a);
        }
    }
}