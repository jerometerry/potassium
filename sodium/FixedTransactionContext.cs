namespace Sodium
{
    using System;

    public sealed class FixedTransactionContext : TransactionContext, IDisposable
    {
        private Transaction transaction;
        private bool disposed;

        public FixedTransactionContext(Transaction transaction)
        {
            this.transaction = transaction;
        }

        public override TA Run<TA>(Func<Transaction, TA> f)
        {
            try
            {
                return f(this.transaction);
            }
            finally
            {
                transaction.Commit();
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (this.transaction != null)
            {
                this.transaction.Dispose();
                this.transaction = null;
            }

            this.disposed = true;
        }
    }
}