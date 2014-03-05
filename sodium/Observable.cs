namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for Event and Behavior
    /// </summary>
    public abstract class Observable<T> : DisposableObject
    {
        private TransactionContext Context
        {
            get
            {
                return TransactionContext.Current;
            }
        }

        /// <summary>
        /// Listen to the Observable for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the event fires</param>
        /// <returns>The event listener</returns>
        public abstract IEventListener<T> Listen(Action<T> callback);

        /// <summary>
        /// Run the given Function using a Transaction obtained from TransactionContext.Current
        /// </summary>
        /// <typeparam name="TR">The return type of the Function</typeparam>
        /// <param name="f">The Function to run</param>
        /// <returns>The result of the Function</returns>
        internal TR StartTransaction<TR>(Func<Transaction, TR> f)
        {
            return this.Context.Run(f);
        }
    }
}
