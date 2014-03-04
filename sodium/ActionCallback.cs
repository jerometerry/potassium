namespace Sodium
{
    using System;

    /// <summary>
    /// ActionCallback wraps an System.Action used to listen to Observables.
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired</typeparam>
    public sealed class ActionCallback<T> : ISodiumCallback<T>
    {
        private readonly Action<T, Scheduler> action;

        /// <summary>
        /// Constructs a new SodiumCallback from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable fires</param>
        public ActionCallback(Action<T, Scheduler> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        /// <param name="firing">The value to be fired to the </param>
        /// <param name="listener">The listener that holds the current callback</param>
        /// <param name="scheduler">The Scheduler used to order the firing</param>
        public void Fire(T firing, IEventListener<T> listener, Scheduler scheduler)
        {
            action(firing, scheduler);
        }
    }
}