namespace JT.Rx.Net.Internal
{
    using System;

    /// <summary>
    /// A TransactionContext is responsible for obtaining a Transaction
    /// used to run functions accepting a Transaction as a parameter.
    /// </summary>
    internal class TransactionContext
    {
        private static readonly TransactionContext CurrentContext = new TransactionContext();

        private static Transaction currentTransaction;

        /// <summary>
        /// Get the current TransactionContext.
        /// </summary>
        public static TransactionContext Current
        {
            get { return CurrentContext; }
        }

        /// <summary>
        /// Run the given function inside a single transaction.
        /// </summary>
        /// <typeparam name="T">The return type of the function</typeparam>
        /// <param name="f">The function to execute</param>
        /// <returns>The return value of the function</returns>
        public T Run<T>(Func<Transaction, T> f)
        {
            lock (Constants.TransactionLock)
            {
                var created = false;
                if (currentTransaction == null)
                {
                    created = true;
                    currentTransaction = new Transaction();
                }

                var v = f(currentTransaction);

                if (created)
                {
                    currentTransaction.Close();
                    currentTransaction.Dispose();
                    currentTransaction = null;
                }

                return v;
            }
        }
    }
}
