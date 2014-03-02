namespace Sodium
{
    using System;

    /// <summary>
    /// FixedTransactionContext is a TransactionContext that uses a single (fixed) Transaction
    /// throughout it's lifetime.
    /// </summary>
    public sealed class FixedTransactionContext : TransactionContext, IDisposable
    {
        private Transaction transaction;
        private bool disposed;

        /// <summary>
        /// Creates a new FixedTransactionContext
        /// </summary>
        /// <param name="transaction">The fixed Transaction</param>
        public FixedTransactionContext(Transaction transaction)
        {
            this.transaction = transaction;
        }

        /// <summary>
        /// Run the given function on the fixed transaction
        /// </summary>
        /// <typeparam name="T">The return value of the supplied function</typeparam>
        /// <param name="f">The function to run on the fixed Transaction</param>
        /// <returns>The result of calling the specified function</returns>
        public override T Run<T>(Func<Transaction, T> f)
        {
            lock (Constants.TransactionLock)
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
        }

        /// <summary>
        /// Releases the current FixedTransactionContext, and it's fixed Transaction.
        /// </summary>
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