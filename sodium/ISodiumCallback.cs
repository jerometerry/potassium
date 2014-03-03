namespace Sodium
{
    /// <summary>
    /// ISodiumCallback is used to fire a value to a registered listener
    /// </summary>
    /// <typeparam name="T">The type of values fired through the associated Event</typeparam>
    public interface ISodiumCallback<T>
    {
        /// <summary>
        /// Fire the given value to the listener
        /// </summary>
        /// <param name="firing">The value to fire</param>
        /// <param name="source">The event that triggered the callback</param>
        /// <param name="transaction">The transaction to use to schedule the firing</param>
        void Fire(T firing, Event<T> source, Transaction transaction);
    }
}