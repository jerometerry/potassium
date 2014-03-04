namespace Sodium
{
    using System;

    /// <summary>
    /// ActionCallback wraps an System.Action used to listen to Observables.
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired</typeparam>
    public sealed class ActionCallback<T> : ISodiumCallback<T>
    {
        private readonly Action<T, ActionScheduler> action;

        /// <summary>
        /// Constructs a new SodiumCallback from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable fires</param>
        public ActionCallback(Action<T, ActionScheduler> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        /// <param name="firing">The value to be fired to the </param>
        /// <param name="listener">The listener that holds the current callback</param>
        /// <param name="scheduler">The Scheduler used to order the firing</param>
        public void Fire(T firing, IEventListener<T> listener, ActionScheduler scheduler)
        {
            action(firing, scheduler);
        }
    }
}