namespace Sodium
{
    /// <summary>
    /// INotification is used to fire a value to a registered subscription
    /// </summary>
    /// <typeparam name="T">The type of values fired through the associated Event</typeparam>
    public interface INotification<in T>
    {
        /// <summary>
        /// Fire the given value to the subscription
        /// </summary>
        /// <param name="value">The value to fire</param>
        /// <param name="transaction">The transaction to use to schedule the firing</param>
        void Send(T value, Transaction transaction);
    }
}