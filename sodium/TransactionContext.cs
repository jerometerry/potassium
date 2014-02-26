namespace Sodium
{
    using System;

    public abstract class TransactionContext
    {
        private static TransactionContext current = new SingleTransactionContext();

        public static TransactionContext Current
        {
            get { return current; }
            set { current = value; }
        }

        public abstract TA Run<TA>(Func<Transaction, TA> f);
    }
}
