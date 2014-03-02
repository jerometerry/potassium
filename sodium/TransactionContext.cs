namespace Sodium
{
    using System;

    /// <summary>
    /// A TransactionContext is responsible for obtaining a Transaction
    /// used to run functions accepting a Transaction as a parameter.
    /// </summary>
    public abstract class TransactionContext
    {
        private static TransactionContext current = new SingleTransactionContext();

        /// <summary>
        /// Get / set the current TransactionContext. The Default TransactionContext 
        /// is a SingleTransactionContext.
        /// </summary>
        public static TransactionContext Current
        {
            get { return current; }
            set { current = value; }
        }

        /// <summary>
        /// Run the given function passing in a located Transaction
        /// </summary>
        /// <typeparam name="T">The return type of the supplied function</typeparam>
        /// <param name="f">The function to run</param>
        /// <returns>The return value of the supplied function</returns>
        public abstract T Run<T>(Func<Transaction, T> f);
    }
}
