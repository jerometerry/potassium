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
        public bool Fire(T firing)
        {
            return this.Run(t => this.Fire(firing, t));
        }

        /// <summary>
        /// Fire the given value to all registered callbacks
        /// </summary>
        /// <param name="transaction">The transaction to invoke the callbacks on</param>
        /// <param name="firing">The value to fire to registered callbacks</param>
        public virtual bool Fire(T firing, Transaction transaction)
        {
            ScheduleClearFirings(transaction);
            AddFiring(firing);
            FireListenerCallbacks(firing, transaction);
            return true;
        }

        /// <summary>
        /// Gets the values that will be sent to newly added
        /// </summary>
        /// <returns>An Array of values that will be fired to all registered listeners</returns>
        protected internal virtual T[] InitialFirings()
        {
            return null;
        }

        protected static TF[] GetInitialFirings<TF>(Event<TF> source)
        {
            var sink = source as EventSink<TF>;
            if (sink == null)
            {
                return null;
            }

            return sink.InitialFirings();
        }

        /// <summary>
        /// Creates a callback that calls the Fire method on the current Event when invoked
        /// </summary>
        /// <returns>In ISodiumCallback that calls Fire, when invoked.</returns>
        protected ISodiumCallback<T> CreateFireCallback()
        {
            return new ActionCallback<T>((t, v) => this.Fire(t, v));
        }

        /// <summary>
        /// Anything fired already in this transaction must be re-fired now so that
        /// there's no order dependency between send and listen.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="listener"></param>
        protected virtual bool Refire(IEventListener<T> listener, Transaction transaction)
        {
            var toFire = firings;
            Fire(listener, toFire, transaction);
            return true;
        }

        protected override IEventListener<T> CreateListener(ISodiumCallback<T> source, Rank superior, Transaction transaction)
        {
            var listener = base.CreateListener(source, superior, transaction);
            InitialFire(listener, transaction);
            Refire(listener, transaction);
            return listener;
        }

        private void InitialFire(IEventListener<T> listener, Transaction transaction)
        {
            var toFire = InitialFirings();
            Fire(listener, toFire, transaction);
        }

        private void Fire(IEventListener<T> listener, ICollection<T> toFire, Transaction transaction)
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
            listener.Callback.Fire(firing, this, transaction);
        }

        private void ScheduleClearFirings(Transaction transaction)
        {
            var noFirings = !firings.Any();
            if (noFirings)
            {
                transaction.Last(() => firings.Clear());
            }
        }

        private void AddFiring(T firing)
        {
            firings.Add(firing);
        }
    }
}
