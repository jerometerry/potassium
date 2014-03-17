using System;
namespace Sodium
{
    public interface IBehavior<T> : IObservable<T>
    {
        Behavior<TB> Apply<TB>(Behavior<Func<T, TB>> bf);
        Behavior<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);
        T GetNewValue();
        Behavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, Behavior<TB> b, Behavior<TC> c);
        Behavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, Behavior<TB> behavior);
        Behavior<TB> Map<TB>(Func<T, TB> map);
        ISubscription<T> SubscribeWithFire(Action<T> callback);
        ISubscription<T> SubscribeWithFire(ISodiumCallback<T> callback, Rank rank);
        T Value { get; }
    }
}
