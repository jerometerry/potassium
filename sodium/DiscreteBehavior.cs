namespace Sodium
{
    /// <summary>
    /// DiscreteBehavior is the base class for Behaviors based on a discrete 
    /// sequence of values, such as Events.
    /// </summary>
    /// <typeparam name="T">The type of value stored in the Behavior</typeparam>
    public abstract class DiscreteBehavior<T> : DisposableObject, IBehavior<T>
    {
        /// <summary>
        /// Sample the current value of the Behavior
        /// </summary>
        public abstract T Value { get; }
    }
}
