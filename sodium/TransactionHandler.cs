using System;

namespace Sodium
{
    public class TransactionHandler<TA> : ITransactionHandler<TA>
    {
        private readonly Action<Transaction, TA> _action;

        public TransactionHandler(Action<Transaction, TA> action)
        {
            _action = action;
        }

        public void Run(Transaction trans, TA a)
        {
            _action(trans, a);
        }
    }
}