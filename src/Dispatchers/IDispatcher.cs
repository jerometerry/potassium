namespace Potassium.Dispatchers
{
    using System;

    /// <summary>
    /// IDispatcher is an interface used to dispatch method calls to specific threads.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Dispatch the action on the appropriate thread
        /// </summary>
        /// <param name="action">The Action to dispatch</param>
        void Dispatch(Action action);
    }
}
