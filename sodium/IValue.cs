namespace Sodium
{
    /// <summary>
    /// IValue is an IEvent that has a current value. The basis of a Behavior, 
    /// without all the bells and whistles
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    public interface IValue<T> : IEvent<T>
    {
        /// <summary>
        /// Gets the current Value
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Gets the new Value
        /// </summary>
        T NewValue { get; }
    }
}
