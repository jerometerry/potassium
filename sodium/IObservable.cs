using System;
namespace Sodium
{
    public interface IObservable<T>
    {
        ISubscription<T> Subscribe(Action<T> callback);

        ISubscription<T> Subscribe(ISodiumCallback<T> callback);

        ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank subscriptionRank);

        ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank superior, Transaction transaction);

        bool CancelSubscription(ISubscription<T> subscription);
    }
}
