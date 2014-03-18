namespace Sodium
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An EventLoop listens for updates from another Event, and forwards them 
    /// to subscriptions of the current event.
    /// </summary>
    /// <typeparam name="T">The type of the value fired through the event</typeparam>
    public class EventLoop<T> : RefireEvent<T>
    {
        private List<ISubscription<T>> subscriptions = new List<ISubscription<T>>();

        /// <summary>
        /// Firings on the given Event will be forwarded to the current Event
        /// </summary>
        /// <param name="source">Event who's firings will be looped to the current Event</param>
        /// <returns>The current EventLoop</returns>
        /// <remarks>Loop can only be called once on an Event. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public ISubscription<T> Loop(IEvent<T> source)
        {
            var subscription = source.Subscribe(this.CreateFireCallback(), Rank);
            subscriptions.Add(subscription);
            return subscription;
        }

        /// <summary>
        /// Disposes the current EventLoop
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            { 
                var clone = new List<ISubscription<T>>(subscriptions);
                subscriptions.Clear();
                foreach (var subscription in clone)
                {
                    subscription.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
