namespace Sodium
{
    /// <summary>
    /// A BehaviorSink is a Behavior that you can fire updates through.
    /// </summary>
    /// <typeparam name="T">The type of value that will be fired through the Behavior</typeparam>
    public sealed class BehaviorSink<T> : Behavior<T>, IFireable<T>
    {
        /// <summary>
        /// Constructs a new BehaviorSink
        /// </summary>
        /// <param name="initialValue">The initial value of the Behavior</param>
        public BehaviorSink(T initialValue) 
            : base(new Sink<T>(), initialValue)
        {
        }
    }
}
