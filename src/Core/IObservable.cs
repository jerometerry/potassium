namespace Potassium.Core
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObservable<T>
    {
        /// <summary>
        /// Subscribe to publications of the current Observable.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Observable fires values.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        ISubscription<T> Subscribe(Action<T> callback);
    }
}