namespace Sodium
{
    using System;

    internal sealed class TransactionLocker<TA> : IDisposable
    {
        private Func<Transaction, TA> code;
        private Transaction previous;
        private Transaction created;
        private bool disposed;
        private bool restored;
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
                    Open();
                    Apply();
                }
                finally
                {
                    this.Close();
                }
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Close();
                code = null;
                disposed = true;
            }
        }

        private void Close()
        {
            if (created != null)
            {
                created.Dispose();
                created = null;
            }
        
            if (!restored)
            {
                Transaction.Current = previous;
                restored = true;
            }
            
            previous = null;
        }

        private void Apply()
        {
            this.result = code(Transaction.Current);
        }

        private void Open()
        {
            // If we are already inside a transaction (which must be on the same
            // thread otherwise we wouldn't have acquired transactionLock), then
            // keep using that same transaction.
            previous = Transaction.Current;

            if (Transaction.Current == null)
            {
                created = new Transaction();
                Transaction.Current = created;
            }
        }
    }
}
