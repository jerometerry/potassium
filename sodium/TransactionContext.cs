namespace Sodium
{
    using System;

    /// <summary>
    /// TransactionContext is used by the Transaction.Run to ensure that only a
    /// single Transaction instance is created.
    /// </summary>
    /// <remarks>Transaction.Run may be invoked recursively indirectly. Hence the
    /// need to ensure that the root Transaction.Run is the only one to create
    /// a new Transaction object.</remarks>
    internal sealed class TransactionContext : IDisposable
    {
        /// <summary>
        /// Transaction for use by the static method Transaction.Run
        /// </summary>
        /// <remarks>
        /// If the Open method initializes current, then the Close method will 
        /// dispose of the created transaction.
        /// 
        /// If current is already initialized when the Open method is invoked,
        /// current is unmodified and the Close method will not dispose current.
        /// 
        /// current is only disposed by the TransactionContext that created it.
        /// </remarks>
        private static Transaction current;

        private Transaction previous;
        private Transaction created;
        private bool restored;
        private bool disposed;

        private TransactionContext()
        {
        }

        public Transaction Transaction
        {
            get
            {
                return current;
            }
        }

        /// <summary>
        /// Run the specified function inside a single transaction
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <param name="f">Function that accepts a transaction and returns a value</param>
        /// <returns></returns>
        /// <remarks>
        /// In most cases this is not needed, because all APIs will create their own
        /// transaction automatically. It is useful where you want to run multiple
        /// reactive operations atomically.
        /// </remarks>
        public static TA Run<TA>(Func<Transaction, TA> f)
        {
            lock (Constants.TransactionLock)
            {
                using (var context = new TransactionContext())
                {
                    context.Open();
                    return f(context.Transaction);
                }
            }
        }

        public void Open()
        {
            // If we are already inside a transaction (which must be on the same
            // thread otherwise we wouldn't have acquired transactionLock), then
            // keep using that same transaction.
            previous = current;

            if (current == null)
            {
                created = new Transaction();
                current = created;
            }
        }

        public void Close()
        {
            if (created != null)
            {
                created.Dispose();
                created = null;
            }
        
            if (!restored)
            {
                current = previous;
                restored = true;
            }
            
            previous = null;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Close();
            disposed = true;
        }
    }
}
