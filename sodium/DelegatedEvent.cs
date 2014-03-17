using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sodium
{
    public abstract class DelegatedEvent<T> : Event<T>
    {
        protected DelegatedEvent(IEvent<T> source)
        {
            this.Source = source;
        }

        /// <summary>
        /// The underlying event that gives the updates for the behavior. If this behavior was created
        /// with a hold, then Source gives you an event equivalent to the one that was held.
        /// </summary>
        protected IEvent<T> Source { get; private set; }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the underlying event fires</param>
        /// <returns>The event subscription</returns>
        public override ISubscription<T> Subscribe(Action<T> callback)
        {
            return this.Source.Subscribe(callback);
        }

        public override ISubscription<T> Subscribe(ISodiumCallback<T> callback)
        {
            return this.Source.Subscribe(callback);
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the underlying event fires</param>
        /// <param name="rank">The rank of the action, used as a superior to the rank of the underlying action.</param>
        /// <returns>The event subscription</returns>
        /// <remarks>Lift converts a function on values to a Behavior on values</remarks>
        public override ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank rank)
        {
            return this.Source.Subscribe(callback, rank);
        }

        /// <summary>
        /// Listen to the underlying event for updates
        /// </summary>
        /// <param name="callback">The action to invoke when the underlying event fires</param>
        /// <param name="rank">The rank of the action, used as a superior to the rank of the underlying action.</param>
        /// <param name="transaction">The transaction used to order actions</param>
        /// <returns>The event subscription</returns>
        public override ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank rank, Transaction transaction)
        {
            return this.Source.Subscribe(callback, rank, transaction);
        }

        public override bool CancelSubscription(ISubscription<T> subscription)
        {
            return this.Source.CancelSubscription(subscription);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Source = null;
            }

            base.Dispose(disposing);
        }
    }
}
