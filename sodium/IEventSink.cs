namespace Sodium
{
    /// <summary>
    /// IEventSink as an IEvent that has a Fire method
    /// </summary>
    /// <typeparam name="T">The type of values fired through the IEvent</typeparam>
    public interface IEventSink<T> : IEvent<T>
    {
        /// <summary>
        /// Fire the given value to all registered subscriptions
        /// </summary>
        /// <param name="firing">The value to be fired</param>
        /// <returns>True if the fire was successful, false otherwise.</returns>
        bool Fire(T firing);
    }
}