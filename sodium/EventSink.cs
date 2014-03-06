namespace Sodium
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An EventSink is an Event that you can fire updates through
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the Event.</typeparam>
    public class EventSink<T> : Event<T>
    {
        /// <summary>
        /// List of values that have been fired on the current Event in the current transaction.
        /// Any listeners that are registered in the current transaction will get fired
        /// these values on registration.
        /// </summary>
        private readonly List<T> firings = new List<T>();

        /// <summary>
        /// Fire the given value to all registered listeners 
        /// </summary>
        /// <param name="firing">The value to be fired</param>
        /// <returns>True if the fire was successful, false otherwise.</returns>
        public bool Send(T firing)
        {
            return this.RunInTransaction(t => this.Send(firing, t));
        }

        internal static TF[] GetInitialFirings<TF>(Event<TF> source)
        {
            var sink = source as EventSink<TF>;
            if (sink == null)
            {
                return null;
            }

            return sink.InitialFirings();
        }

        /// <summary>
        /// Fire the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="firing">The value to fire to registered callbacks</param>
        internal virtual bool Send(T firing, Transaction transaction)
        {
            ScheduleClearFirings(transaction);
            AddFiring(firing);
            FireListenerCallbacks(firing, transaction);
            return true;
        }

        /// <summary>
        /// Creates a callback that calls the Fire method on the current Event when invoked
        /// </summary>
        /// <returns>In ISodiumCallback that calls Fire, when invoked.</returns>
        internal ISodiumCallback<T> CreateFireCallback()
        {
            return new ActionCallback<T>((t, v) => this.Send(t, v));
        }

        /// <summary>
        /// Anything fired already in this transaction must be re-fired now so that
        /// there's no order dependency between send and listen.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="listener"></param>
        internal virtual bool Refire(IEventListener<T> listener, Transaction transaction)
        {
            var toFire = firings;
            this.Send(listener, toFire, transaction);
            return true;
        }

        internal override IEventListener<T> CreateListener(ISodiumCallback<T> source, Rank superior, Transaction transaction)
        {
            var listener = base.CreateListener(source, superior, transaction);
            InitialFire(listener, transaction);
            Refire(listener, transaction);
            return listener;
        }

        /// <summary>
        /// Gets the values that will be sent to newly added
        /// </summary>
        /// <returns>An Array of values that will be fired to all registered listeners</returns>
        protected internal virtual T[] InitialFirings()
        {
            return null;
        }

        private void InitialFire(IEventListener<T> listener, Transaction transaction)
        {
            var toFire = InitialFirings();
            this.Send(listener, toFire, transaction);
        }

        private void Send(IEventListener<T> listener, ICollection<T> toFire, Transaction transaction)
        {
            if (toFire == null || toFire.Count == 0)
            {
                return;
            }

            foreach (var firing in toFire)
            {
                FireListenerCallback(firing, listener, transaction);
            }
        }

        private void FireListenerCallbacks(T firing, Transaction transaction)
        {
            var clone = new List<IEventListener<T>>(Listeners);
            foreach (var listener in clone)
            {
                FireListenerCallback(firing, listener, transaction);
            }
        }

        private void FireListenerCallback(T firing, IEventListener<T> listener, Transaction transaction)
        {
            var l = (EventListener<T>)listener;
            l.Callback.Fire(firing, listener, transaction);
        }

        private void ScheduleClearFirings(Transaction transaction)
        {
            var noFirings = !firings.Any();
            if (noFirings)
            {
                transaction.Medium(() => firings.Clear());
            }
        }

        private void AddFiring(T firing)
        {
            firings.Add(firing);
        }
    }
}
