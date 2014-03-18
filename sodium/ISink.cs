namespace Sodium
{
    public interface ISink<T> : IObservable<T>
    {
        /// <summary>
        /// Fire the given value to all registered subscriptions
        /// </summary>
        /// <param name="firing">The value to be fired</param>
        /// <returns>True if the fire was successful, false otherwise.</returns>
        bool Fire(T firing);
    }
}