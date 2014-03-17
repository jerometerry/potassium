using System;
namespace Sodium
{
    public interface IEvent<T> : IObservable<T>, ISnapshot<T>, IHoldable<T>
    {
        IBehavior<TS> Accum<TS>(TS initState, Func<T, TS, TS> snapshot);

        IEvent<T> Coalesce(Func<T, T, T> coalesce);
        
        IEvent<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);
        
        IEvent<T> Delay();
        
        IEvent<T> Filter(Func<T, bool> predicate);
        
        IEvent<T> FilterNotNull();

        IEvent<T> Gate(IValue<bool> predicate);
        
        IEvent<TB> Map<TB>(Func<T, TB> map);
        
        IEvent<T> Merge(IObservable<T> source);

        IEvent<T> Merge(IObservable<T> source, Func<T, T, T> coalesce);
        
        IEvent<T> Once();
    }
}
