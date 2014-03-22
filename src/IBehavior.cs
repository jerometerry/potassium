namespace JT.Rx.Net
{
    /// <summary>
    /// A Behavior is a continuous, time varying value.
    /// </summary>
    /// <typeparam name="T">The type of value of the Behavior.</typeparam>
    public interface IBehavior<out T>
    {
        /// <summary>
        /// Sample the behavior's current value
        /// </summary>
        T Value
        {
            get;
        }
    }
}
