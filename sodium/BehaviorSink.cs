namespace Sodium
{
    /// <summary>
    /// A BehaviorSink is a Behavior that you can fire updates through.
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired through the Behavior</typeparam>
    public class BehaviorSink<T> : Behavior<T>
    {
        /// <summary>
        /// Constructs a new BehaviorSink
        /// </summary>
        /// <param name="initialValue">The initial value of the Behavior</param>
        public BehaviorSink(T initialValue) 
            : base(new EventSink<T>(), initialValue)
        {
        }

        /// <summary>
        /// Fire the given value on the underlying Event
        /// </summary>
        /// <param name="firing">The value to fire</param>
        /// <returns>True if fired, false otherwise</returns>
        public bool Fire(T firing)
        {
            return ((EventSink<T>)Source).Send(firing);
        }
    }
}
