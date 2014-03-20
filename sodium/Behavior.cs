namespace Sodium
{
    /// <summary>
    /// A Behavior is a continuous, time varying value.
    /// </summary>
    /// <typeparam name="T">The type of value of the Behavior.</typeparam>
    public abstract class Behavior<T> : DisposableObject
    {
        /// <summary>
        /// Sample the behavior's current value
        /// </summary>
        public abstract T Value
        {
            get;
        }
    }
}
