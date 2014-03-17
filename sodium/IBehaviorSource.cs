namespace Sodium
{
    using System;

    /// <summary>
    /// Interface required for sources for use with Behaviors
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Behavior.</typeparam>
    public interface IBehaviorSource<T> : IObservable<T>, IDisposable
    {
        /// <summary>
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <param name="coalesce">A function that takes two firings of the same type, and returns
        /// produces a new firing of the same type.</param>
        /// <returns>A new Event that fires the coalesced values</returns>
        /// <remarks>
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        ISnapshot<T> Coalesce(Func<T, T, T> coalesce);

        /// <summary>
        /// Map firings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that fires whenever the current Event fires, the
        /// the mapped value is computed using the supplied mapping.</returns>
        IHoldable<TB> Map<TB>(Func<T, TB> map);
    }
}
