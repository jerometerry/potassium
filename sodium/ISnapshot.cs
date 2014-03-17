namespace Sodium
{
    using System;

    /// <summary>
    /// ISnapshot is an Observable that can be take snapshots
    /// </summary>
    /// <typeparam name="T">The type of value fired through the Observable</typeparam>
    public interface ISnapshot<T> : IDisposable
    {
        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <param name="behavior">The Behavior to sample when calculating the snapshot</param>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <returns>A new Event that will produce the snapshot when the current event fires</returns>
        Event<TC> Snapshot<TB, TC>(Behavior<TB> behavior, Func<T, TB, TC> snapshot);
    }
}
