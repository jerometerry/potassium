namespace Potassium.Core
{    
    using System;

    /// <summary>
    /// An EventRepeater is an Event that can be fed values from another Event
    /// </summary>
    /// <typeparam name="T">The type of the value fired through the event</typeparam>
    /// <remarks>An EventRepeater can only be fed values from one Event. 
    /// If Repeat is Invoked multiple times, an exception will be raised.</remarks>
    public class EventRepeater<T> : RefireEvent<T>
    {
        private ISubscription<T> subscription;

        /// <summary>
        /// Constructs a new EventRepeater, that's not yet repeating any Events.
        /// </summary>
        /// <remarks>Call the Repeat method to start Repeating another Event</remarks>
        public EventRepeater()
        {
        }

        /// <summary>
        /// Constructs a new EventRepeater, repeating the specified Event
        /// </summary>
        /// <param name="source">The Event to repeat</param>
        public EventRepeater(Observable<T> source)
        {
            this.Repeat(source);
        }

        /// <summary>
        /// Firings on the given Event will be forwarded to the current Event
        /// </summary>
        /// <param name="source">Event who's firings will be looped to the current Event</param>
        /// <returns>The current EventRepeater</returns>
        /// <remarks>Loop can only be called once on an Event. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public EventRepeater<T> Repeat(Observable<T> source)
        {
            if (subscription != null)
            {
                throw new ApplicationException("EventRepeater is already been fed from another Event.");
            }

            var forward = this.Repeat();
            this.subscription = source.Subscribe(forward, this.Priority);
            return this;
        }

        /// <summary>
        /// Disposes the current EventRepeater
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (subscription != null)
            {
                subscription.Dispose();
                subscription = null;
            }

            base.Dispose(disposing);
        }
    }
}
