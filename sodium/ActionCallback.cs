namespace Sodium
{
    using System;

    /// <summary>
    /// ActionCallback wraps an System.Action used to listen to Observables.
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired</typeparam>
    public sealed class ActionCallback<T> : ISodiumCallback<T>
    {
        private readonly Action<T, Transaction> action;

        /// <summary>
        /// Constructs a new SodiumCallback from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable fires</param>
        public ActionCallback(Action<T, Transaction> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        /// <param name="firing">The value to be fired to the </param>
        /// <param name="source">The Event that triggered the callback</param>
        /// <param name="transaction">The Transaction used to order the firing</param>
        public void Fire(T firing, Event<T> source, Transaction transaction)
        {
            action(firing, transaction);
        }
    }
}