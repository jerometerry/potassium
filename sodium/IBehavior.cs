namespace Sodium
{
    using System;

    public interface IBehavior<T> : IValue<T>
    {
        IBehavior<TB> Apply<TB>(IBehavior<Func<T, TB>> bf);
        
        IBehavior<TB> CollectB<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);
        
        IBehavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, IBehavior<TB> b, IBehavior<TC> c);
        
        IBehavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, IBehavior<TB> behavior);
        
        IBehavior<TB> MapB<TB>(Func<T, TB> map);

        ISubscription<T> SubscribeAndFire(Action<T> callback);

        ISubscription<T> SubscribeAndFire(ISodiumCallback<T> callback, Rank rank);
    }   
}
