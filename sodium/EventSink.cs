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
        /// List of values that have been fired on the current Event in the current scheduler.
        /// Any listeners that are registered in the current scheduler will get fired
        /// these values on registration.
        /// </summary>
        private readonly List<T> firings = new List<T>();

        /// <summary>
        /// Fire the given value to all registered listeners 
        /// </summary>
        /// <param name="firing">The value to be fired</param>
        public bool Fire(T firing)
        {
            return this.StartScheduler(t => this.Fire(firing, t));
        }

        /// <summary>
        /// Fire the given value to all registered callbacks
        /// </summary>
        /// <param name="scheduler">The scheduler to invoke the callbacks on</param>
        /// <param name="firing">The value to fire to registered callbacks</param>
        public virtual bool Fire(T firing, ActionScheduler scheduler)
        {
            ScheduleClearFirings(scheduler);
            AddFiring(firing);
            FireListenerCallbacks(firing, scheduler);
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
        /// Anything fired already in this scheduler must be re-fired now so that
        /// there's no order dependency between send and listen.
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="listener"></param>
        protected virtual bool Refire(IEventListener<T> listener, ActionScheduler scheduler)
        {
            var toFire = firings;
            Fire(listener, toFire, scheduler);
            return true;
        }

        protected override IEventListener<T> CreateListener(ISodiumCallback<T> source, Rank superior, ActionScheduler scheduler)
        {
            var listener = base.CreateListener(source, superior, scheduler);
            InitialFire(listener, scheduler);
            Refire(listener, scheduler);
            return listener;
        }

        private void InitialFire(IEventListener<T> listener, ActionScheduler scheduler)
        {
            var toFire = InitialFirings();
            Fire(listener, toFire, scheduler);
        }

        private void Fire(IEventListener<T> listener, ICollection<T> toFire, ActionScheduler scheduler)
        {
            if (toFire == null || toFire.Count == 0)
            {
                return;
            }

            foreach (var firing in toFire)
            {
                FireListenerCallback(firing, listener, scheduler);
            }
        }

        private void FireListenerCallbacks(T firing, ActionScheduler scheduler)
        {
            var clone = new List<IEventListener<T>>(Listeners);
            foreach (var listener in clone)
            {
                FireListenerCallback(firing, listener, scheduler);
            }
        }

        private void FireListenerCallback(T firing, IEventListener<T> listener, ActionScheduler scheduler)
        {
            listener.Callback.Fire(firing, listener, scheduler);
        }

        private void ScheduleClearFirings(ActionScheduler scheduler)
        {
            var noFirings = !firings.Any();
            if (noFirings)
            {
                scheduler.Medium(() => firings.Clear());
            }
        }

        private void AddFiring(T firing)
        {
            firings.Add(firing);
        }
    }
}
