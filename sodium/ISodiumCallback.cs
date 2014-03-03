namespace Sodium
{
    /// <summary>
    /// ISodiumCallback is used to fire a value to a registered listener
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISodiumCallback<in T>
    {
        /// <summary>
        /// Fire the given value to the listener
        /// </summary>
        /// <param name="firing">The value to fire</param>
        /// <param name="transaction">The transaction to use to schedule the firing</param>
        void Fire(T firing, Transaction transaction);
    }
}