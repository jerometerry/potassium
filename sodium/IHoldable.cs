namespace Sodium
{
    using System;

    /// <summary>
    /// IHoldable is an Observable that can be held. I.e. converted to a behavior.
    /// </summary>
    /// <typeparam name="T">The type of value that is fired through the Behavior</typeparam>
    public interface IHoldable<T> : IDisposable
    {
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
    }
}
