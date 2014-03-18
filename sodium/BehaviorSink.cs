namespace Sodium
{
    /// <summary>
    /// BehaviorSink is a Behavior who's underlying Event is an EventSink
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Behavior</typeparam>
    public class BehaviorSink<T> : Behavior<T>
    {
        /// <summary>
        /// Constructs a new BehaviorSink
        /// </summary>
        /// <param name="initValue">The initial value of the Behavior</param>
        public BehaviorSink(T initValue)
            : base(new EventSink<T>(), initValue)
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Fire the given value to all registered subscriptions
        /// </summary>
        /// <param name="firing">The value to be fired</param>
        /// <returns>True if the fire was successful, false otherwise.</returns>
        public bool Fire(T firing)
        {
            var sink = (EventSink<T>)this.Source;
            return sink.Fire(firing);
        }
    }
}
