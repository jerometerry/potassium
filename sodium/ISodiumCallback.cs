namespace Sodium
{
    /// <summary>
    /// ISodiumCallback is used to fire a value to a registered subscription
    /// </summary>
    /// <typeparam name="T">The type of values fired through the associated Event</typeparam>
    public interface ISodiumCallback<T>
    {
        /// <summary>
        /// Fire the given value to the subscription
        /// </summary>
        /// <param name="value">The value to fire</param>
        /// <param name="subscription">The subscription that holds the current callback</param>
        /// <param name="transaction">The transaction to use to schedule the firing</param>
        void Invoke(T value, ISubscription<T> subscription, Transaction transaction);
    }
}