﻿namespace Sodium
{
    internal sealed class TransactionContext
    {
        private static Transaction current;
        private Transaction previous;
        private Transaction created;
        private bool restored;

        public Transaction Transaction
        {
            get
            {
                return current;
            }
        }

        public void Close()
        {
            if (created != null)
            {
                created.Close();
                created = null;
            }
        
            if (!restored)
            {
                current = previous;
                restored = true;
            }
            
            previous = null;
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
    }
}
