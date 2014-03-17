using System;
namespace Sodium
{
    public interface IBehavior<T> : IObservable<T>
    {
        IBehavior<TB> Apply<TB>(IBehavior<Func<T, TB>> bf);
        IBehavior<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);
        T GetNewValue();
        IBehavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, IBehavior<TB> b, IBehavior<TC> c);
        IBehavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, IBehavior<TB> behavior);
        IBehavior<TB> Map<TB>(Func<T, TB> map);
        ISubscription<T> SubscribeWithFire(Action<T> callback);
        ISubscription<T> SubscribeWithFire(ISodiumCallback<T> callback, Rank rank);
        T Value { get; }

        IEvent<T> Values(Transaction transaction);
    }
}
