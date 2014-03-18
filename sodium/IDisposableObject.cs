namespace Sodium
{
    using System;

    /// <summary>
    /// IDisposableObject contains properties to determine the disposal state,
    /// and to chain together disposing of related objects.
    /// </summary>
    public interface IDisposableObject : IDisposable
    {
        /// <summary>
        /// Gets whether the current IDisposableObject is disposed
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// Gets whether the current IDisposableObject is being disposed
        /// </summary>
        bool Disposing { get; }

        /// <summary>
        /// Register the given IDisposable to be disposed when the current IDisposableObject is disposed.
        /// </summary>
        /// <param name="o">The object to be registered for disposal</param>
        void Register(IDisposable o);
    }
}
