namespace Potassium.Dispatchers
{
    using System;

    /// <summary>
    /// CurrentThreadDispatcher is an IDispatcher that invokes methods on the current thread
    /// </summary>
    public class CurrentThreadDispatcher : IDispatcher
    {
        /// <summary>
        /// Invoke the action on the current thread
        /// </summary>
        /// <param name="action">The action to invoke</param>
        public void Dispatch(Action action)
        {
            action();
        }
    }
}
