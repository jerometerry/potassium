using System;
namespace Sodium
{
    public interface IObservable<T>
    {
        ISubscription<T> Subscribe(Action<T> callback);
    }
}
