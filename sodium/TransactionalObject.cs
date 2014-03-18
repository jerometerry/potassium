namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for objects requiring starting of new Transactions
    /// </summary>
    public class TransactionalObject : DisposableObject
    {
        private TransactionContext Context
        {
            get
            {
                return TransactionContext.Current;
            }
        }

        /// <summary>
        /// Run the given Function using a Transaction obtained from TransactionContext.Current
        /// </summary>
        /// <typeparam name="TR">The return type of the Function</typeparam>
        /// <param name="f">The Function to run</param>
        /// <returns>The result of the Function</returns>
        protected TR StartTransaction<TR>(Func<Transaction, TR> f)
        {
            return this.Context.Run(f);
        }
    }
}
