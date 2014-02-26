namespace Sodium
{
    using System;

    /// <summary>
    /// SingleTransactionContext is used to ensure that only a single Transaction instance is created.
    /// </summary>
    /// <remarks>SingleTransactionContext.Run may be invoked recursively indirectly. Hence the
    /// need to ensure that the root TransactionContext.Current.Run is the only one to create
    /// a new Transaction object.</remarks>
    public sealed class SingleTransactionContext : TransactionContext
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
        public override TA Run<TA>(Func<Transaction, TA> f)
        {
            lock (Constants.TransactionLock)
            {
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                var created = false;
                if (current == null)
                {
                    current = new Transaction();
                    created = true;
                }

                try
                {
                    return f(current);
                }
                finally
                {
                    if (created)
                    {
                        current.Commit();
                        current.Dispose();
                        current = null;
                    }
                }
            }
        }
    }
}