using System;
namespace Sodium
{
    public interface IEvent<T> : IObservable<T>, ISnapshot<T>
    {
        Behavior<TS> Accum<TS>(TS initState, Func<T, TS, TS> snapshot);
        bool CancelSubscription(ISubscription<T> subscription);
        Event<T> Coalesce(Func<T, T, T> coalesce);
        Event<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);
        Event<T> Delay();
        Event<T> Filter(Func<T, bool> predicate);
        Event<T> FilterNotNull();
        Event<T> Gate(Behavior<bool> predicate);
        Behavior<T> Hold(T initValue);
        Event<TB> Map<TB>(Func<T, TB> map);
        Event<T> Merge(Event<T> source);
        Event<T> Merge(Event<T> source, Func<T, T, T> coalesce);
        Event<T> Once();
        Event<TB> Snapshot<TB>(Behavior<TB> behavior);
    }
}
