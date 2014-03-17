namespace Sodium
{
    using System;

    public interface IObservableBehavior<T> : IObservable<T>
    {
        ISubscription<T> SubscribeWithFire(Action<T> callback);

        ISubscription<T> SubscribeWithFire(ISodiumCallback<T> callback, Rank rank);
    }
}
