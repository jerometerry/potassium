namespace Sodium
{
    using System;

    /// <summary>
    /// ISubscription is the return value of Event.Subscribe, and is used to stop 
    /// the Event from firing the action that was passed to the Listen method.
    /// </summary>
    /// <typeparam name="T">The type of values that are fired through the event being subscribed to.</typeparam>
    /// <remarks>To stop the ISubscription from receiving updates from the Event,
    /// call the Dispose method. The event will be disposed when the number of subscriptions reaches zero.
    /// </remarks>
    public interface ISubscription<T> : IDisposable
    {
        /// <summary>
        /// Gets the Event the current ISubscription is listening to
        /// </summary>
        IEvent<T> Source { get; }
    }
}