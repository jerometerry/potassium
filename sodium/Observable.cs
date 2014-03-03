namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for Event and Behavior
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired through the Observable.</typeparam>
    public abstract class Observable<T> : SodiumObject
    {
        /// <summary>
        /// Listen to the current observable, which fires immediately after listening
        /// </summary>
        /// <param name="callback">The action to invoke when the observable fires</param>
        /// <returns>The newly created listener.</returns>
        public abstract IEventListener<T> Listen(Action<T> callback);

        /// <summary>
        /// Listen to the current observable, which fires immediately after listening
        /// </summary>
        /// <param name="callback">The action to invoke when the observable fires</param>
        /// <returns>The newly created listener.</returns>
        public abstract IEventListener<T> Listen(ISodiumCallback<T> callback);

        /// <summary>
        /// Listen to the current observable, but without immediate firing. 
        /// </summary>
        /// <param name="callback">The action to invoke when the observable fires</param>
        /// <returns>The newly created listener</returns>
        public abstract IEventListener<T> ListenSuppressed(Action<T> callback);

        /// <summary>
        /// Listen to the current observable, but without immediate firing. 
        /// </summary>
        /// <param name="callback">The action to invoke when the observable fires</param>
        /// <returns>The newly created listener</returns>
        public abstract IEventListener<T> ListenSuppressed(ISodiumCallback<T> callback);

        /// <summary>
        /// Fires the given value to all registered listeners
        /// </summary>
        /// <param name="a">The value to fire.</param>
        public abstract bool Fire(T a);

        /// <summary>
        /// Run the given Action using a Transaction obtained from TransactionContext.Current
        /// </summary>
        /// <param name="action">The Action to run</param>
        protected void Run(Action<Transaction> action)
        {
            this.Run(t => { action(t); return Unit.Nothing; });
        }

        /// <summary>
        /// Run the given Function using a Transaction obtained from TransactionContext.Current
        /// </summary>
        /// <typeparam name="TR">The return type of the Function</typeparam>
        /// <param name="f">The Function to run</param>
        /// <returns>The result of the Function</returns>
        protected TR Run<TR>(Func<Transaction, TR> f)
        {
            return TransactionContext.Current.Run(f);
        }
    }
}
