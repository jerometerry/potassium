namespace Sodium
{
    internal abstract class TransactionLocker
    {
        /// <summary>
        /// Coarse-grained lock that's held during the whole transaction. 
        /// </summary>
        private static readonly object TransactionLock = new object();
        private static Transaction current;

        private Transaction previous;
        private Transaction transaction;
        private bool newTransactionCreated;

        public void Run()
        {
            lock (TransactionLock)
            {
                try
                {
                    Apply(OpenTransaction());
                }
                finally
                {
                    CloseTransaction();
                    RestoreTransaction();
                }
            }
        }

        protected abstract void Apply(Transaction t);

        private Transaction OpenTransaction()
        {
            // If we are already inside a transaction (which must be on the same
            // thread otherwise we wouldn't have acquired transactionLock), then
            // keep using that same transaction.
            previous = current;

            if (current == null)
            {
                transaction = new Transaction();
                current = transaction;
                newTransactionCreated = true;
            }
            else
            {
                transaction = current;
            }

            return transaction;
        }

        private void CloseTransaction()
        {
            if (newTransactionCreated)
            {
                current.Close();
            }
        }

        private void RestoreTransaction()
        {
            current = previous;
        }
    }
}
