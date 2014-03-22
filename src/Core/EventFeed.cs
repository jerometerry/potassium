namespace JT.Rx.Net
{
    
    using System;

    /// <summary>
    /// An EventFeed is an Event that can be fed values from another Event
    /// </summary>
    /// <typeparam name="T">The type of the value published through the event</typeparam>
    /// <remarks>An EventFeed can only be fed values from one Event. 
    /// If Feed is Invoked multiple times, an exception will be raised.</remarks>
    public class EventFeed<T> : SubscribeRepublishEvent<T>
    {
        private ISubscription<T> subscription;

        /// <summary>
        /// Firings on the given Event will be forwarded to the current Event
        /// </summary>
        /// <param name="source">Event who's publishings will be looped to the current Event</param>
        /// <returns>The current EventFeed</returns>
        /// <remarks>Loop can only be called once on an Event. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public ISubscription<T> Feed(Observable<T> source)
        {
            if (subscription != null)
            {
                throw new ApplicationException("EventFeed is already been fed from another Event.");
            }

            var target = this.CreateSubscriptionPublisher();
            this.subscription = source.Subscribe(target, Priority);
            return subscription;
        }

        /// <summary>
        /// Disposes the current EventFeed
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
