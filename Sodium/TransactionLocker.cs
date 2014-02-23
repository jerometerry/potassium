namespace Sodium
{
    using System;

    internal sealed class TransactionLocker<TA>
    {
        private readonly Func<Transaction, TA> code;

        private Transaction previous;
        private Transaction transaction;
        private bool newTransactionCreated;
        
        private TA result;

        public TransactionLocker(Func<Transaction, TA> code)
        {
            this.code = code;
        }

        public TA Result
        {
            get { return result; }
        }

        public void Run()
        {
            lock (Transaction.TransactionLock)
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

        private void Apply(Transaction t)
        {
            this.result = code(t);
        }

        private Transaction OpenTransaction()
        {
            // If we are already inside a transaction (which must be on the same
            // thread otherwise we wouldn't have acquired transactionLock), then
            // keep using that same transaction.
            previous = Transaction.Current;

            if (Transaction.Current == null)
            {
                transaction = new Transaction();
                Transaction.Current = transaction;
                newTransactionCreated = true;
            }
            else
            {
                transaction = Transaction.Current;
            }

            return transaction;
        }

        private void CloseTransaction()
        {
            if (newTransactionCreated)
            {
                Transaction.Current.Close();
            }
        }

        private void RestoreTransaction()
        {
            Transaction.Current = previous;
        }
    }
}
