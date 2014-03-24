namespace Potassium.Dispatchers
{
    using System;

    /// <summary>
    /// CurrentThreadDispatcher is an IDispatcher that invokes methods on the current thread
    /// </summary>
    public class CurrentThreadDispatcher : IDispatcher
    {
        public void Dispatch(Action action)
        {
            action();
        }
    }
}
