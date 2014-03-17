namespace Sodium
{
    using System;

    /// <summary>
    /// IFiringObservable is an IObservable that has the ability to fire initial values
    /// during the subscription process.
    /// </summary>
    /// <typeparam name="T">The value fired through the observable</typeparam>
    public interface IFiringObservable<T> : IObservable<T>
    {
        ISubscription<T> SubscribeAndFire(Action<T> callback);

        ISubscription<T> SubscribeAndFire(ISodiumCallback<T> callback, Rank rank);
    }
}
