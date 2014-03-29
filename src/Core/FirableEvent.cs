namespace Potassium.Core
{
    using Potassium.Internal;    

    /// <summary>
    /// An FirableEvent is an Event that allows callers to fire values to subscribers.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    public class FirableEvent<T> : Event<T>
    {
        /// <summary>
        /// Fire the given value to all registered subscriptions
        /// </summary>
        /// <param name="value">The value to be fired</param>
        /// <returns>True if the fire was successful, false otherwise.</returns>
        public bool Fire(T value)
        {
            return Transaction.Start(t => this.Fire(value, t));
        }

        /// <summary>
        /// Creates a Observer that repeats events from an Observable to the current FirableEvent
        /// </summary>
        /// <returns>An Observer that calls Fire, when invoked.</returns>
        internal Observer<T> Repeat()
        {
            return new Observer<T>((t, v) => this.Fire(t, v));
        }

        /// <summary>
        /// Fire the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="value">The value to fire to registered callbacks</param>
        internal virtual bool Fire(T value, Transaction transaction)
        {
            var clone = this.Subscriptions.ToArray();
            Observer<T>.Notify(value, clone, transaction);
            return true;
        }
    }
}
