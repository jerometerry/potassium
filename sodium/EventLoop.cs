namespace Sodium
{
    using System;

    /// <summary>
    /// An EventLoop listens for updates from another Event, and forwards them 
    /// to subscriptions of the current event.
    /// </summary>
    /// <typeparam name="T">The type of the value fired through the event</typeparam>
    public class EventLoop<T> : RefireEventSink<T>
    {
        private Observable<T> source;
        private ISubscription<T> subscription;

        /// <summary>
        /// Firings on the given Event will be forwarded to the current Event
        /// </summary>
        /// <param name="toLoop">Event who's firings will be looped to the current Event</param>
        /// <returns>The current EventLoop</returns>
        /// <remarks>Loop can only be called once on an Event. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public Event<T> Loop(Observable<T> toLoop)
        {
            if (this.source != null)
            {
                throw new ApplicationException("EventLoop looped more than once");
            }

            this.source = toLoop;
            this.subscription = source.Subscribe(this.CreateFireCallback(), Rank);
            return this;
        }

        /// <summary>
        /// Disposes the current EventLoop
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            this.source = null;

            base.Dispose(disposing);
        }
    }
}
