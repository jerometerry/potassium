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
    }
}
