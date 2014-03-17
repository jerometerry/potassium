using System;
namespace Sodium
{
    public interface IEvent<T> : IObservable<T>, ISnapshot<T>, IHoldable<T>
    {
        Behavior<TS> Accum<TS>(TS initState, Func<T, TS, TS> snapshot);
        bool CancelSubscription(ISubscription<T> subscription);
        Event<T> Coalesce(Func<T, T, T> coalesce);
        IEvent<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);
        IEvent<T> Delay();
        Event<T> Filter(Func<T, bool> predicate);
        Event<T> FilterNotNull();
        IEvent<T> Gate(Behavior<bool> predicate);
        IEvent<TB> Map<TB>(Func<T, TB> map);
        IEvent<T> Merge(IEvent<T> source);
        IEvent<T> Merge(IEvent<T> source, Func<T, T, T> coalesce);
        IEvent<T> Once();
    }
}
