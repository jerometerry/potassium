namespace Sodium
{
    /// <summary>
    /// An BehaviorPublisher is an Behavior that allows callers to publish values to subscribers.
    /// </summary>
    /// <typeparam name="T">The type of values published through the Behavior</typeparam>
    public class BehaviorPublisher<T> : EventBasedBehavior<T>
    {
        /// <summary>
        /// Constructs a new BehaviorPublisher
        /// </summary>
        /// <param name="initValue">The initial value of the Behavior</param>
        public BehaviorPublisher(T initValue)
            : base(new EventPublisher<T>(), initValue)
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Publish the given value to all registered subscriptions
        /// </summary>
        /// <param name="value">The value to be published</param>
        /// <returns>True if the publish was successful, false otherwise.</returns>
        public bool Publish(T value)
        {
            var sink = (EventPublisher<T>)this.Source;
            return sink.Publish(value);
        }
    }
}
