using System;

namespace sodium
{
    public class TransactionHandler<A> : ITransactionHandler<A>
    {
        private readonly Action<Transaction, A> _action;

        public TransactionHandler(Action<Transaction, A> action)
        {
            _action = action;
        }

        public void Run(Transaction trans, A a)
        {
            _action(trans, a);
        }
    }
}