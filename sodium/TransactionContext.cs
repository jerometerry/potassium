namespace Sodium
{
    using System;

    /// <summary>
    /// TransactionContext is used to ensure that only a single Transaction instance is created.
    /// </summary>
    /// <remarks>TransactionContext.Run may be invoked recursively indirectly. Hence the
    /// need to ensure that the root TransactionContext.Run is the only one to create
    /// a new Transaction object.</remarks>
    internal static class TransactionContext
    {
        private static Transaction current;

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
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                var previous = current;
                Transaction created = null;

                if (current == null)
                {
                    created = new Transaction();
                    current = created;
                }

                try
                {
                    return f(current);
                }
                finally 
                {
                    if (created != null)
                    {
                        created.Dispose();
                        current = previous;
                    }
                }
            }
        }
    }
}
