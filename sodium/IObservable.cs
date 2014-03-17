namespace Sodium
{
    using System;

    public interface IObservable<T> : IDisposableObject
    {
        IBehavior<TS> Accum<TS>(TS initState, Func<T, TS, TS> snapshot);

        IObservable<T> Coalesce(Func<T, T, T> coalesce);

        IObservable<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);

        IObservable<T> Delay();

        IObservable<T> Filter(Func<T, bool> predicate);

        IObservable<T> FilterNotNull();

        IObservable<T> Gate(IValue<bool> predicate);

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        /// </summary>
        /// <param name="initValue">The initial value for the Behavior</param>
        /// <returns>A Behavior that updates when the current event is fired,
        /// having the specified initial value.</returns>
        IBehavior<T> Hold(T initValue);

        IObservable<TB> Map<TB>(Func<T, TB> map);

        IObservable<T> Merge(IObservable<T> source);

        IObservable<T> Merge(IObservable<T> source, Func<T, T, T> coalesce);

        IObservable<T> Once();

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <param name="valueStream">The Behavior to sample when calculating the snapshot</param>
        /// <returns>A new Event that will produce the snapshot when the current event fires</returns>
        IObservable<TB> Snapshot<TB>(IValue<TB> valueStream);

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <param name="valueStream">The Behavior to sample when calculating the snapshot</param>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <returns>A new Event that will produce the snapshot when the current event fires</returns>
        IObservable<TC> Snapshot<TB, TC>(IValue<TB> valueStream, Func<T, TB, TC> snapshot);

        ISubscription<T> Subscribe(Action<T> callback);

        ISubscription<T> Subscribe(ISodiumCallback<T> callback);

        ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank subscriptionRank);

        ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank superior, Transaction transaction);

        bool CancelSubscription(ISubscription<T> subscription);
    }
}
