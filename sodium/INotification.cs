namespace Sodium
{
    /// <summary>
    /// INotification is used to publish a value to a registered subscription
    /// </summary>
    /// <typeparam name="T">The type of values published through the associated Event</typeparam>
    public interface INotification<in T>
    {
        /// <summary>
        /// Publish the given value to the subscription
        /// </summary>
        /// <param name="value">The value to publish</param>
        /// <param name="transaction">The transaction to use to schedule the publishing</param>
        void Send(T value, Transaction transaction);
    }
}