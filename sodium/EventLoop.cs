namespace Sodium
{
    using System;

    /// <summary>
    /// An EventLoop listens for updates from another Event, and forwards them 
    /// to subscriptions of the current event.
    /// </summary>
    /// <typeparam name="T">The type of the value published through the event</typeparam>
    public class EventLoop<T> : SubscribeRepublishEvent<T>
    {
        private ISubscription<T> subscription;

        /// <summary>
        /// Firings on the given Event will be forwarded to the current Event
        /// </summary>
        /// <param name="source">Event who's publishings will be looped to the current Event</param>
        /// <returns>The current EventLoop</returns>
        /// <remarks>Loop can only be called once on an Event. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public ISubscription<T> Loop(IObservable<T> source)
        {
            if (subscription != null)
            {
                throw new ApplicationException("EventLoop has already been looped.");
            }

            this.subscription = source.Subscribe(this.CreatePublishCallback(), Rank);
            return subscription;
        }

        /// <summary>
        /// Disposes the current EventLoop
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            { 
                if (subscription != null)
                {
                    subscription.Dispose();
                    subscription = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
