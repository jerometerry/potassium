namespace Sodium
{
    using System;

    /// <summary>
    /// An EventLoop listens for updates from another Event, and forwards them 
    /// to listeners of the current event.
    /// </summary>
    /// <typeparam name="T">The type of the value fired through the event</typeparam>
    public class EventLoop<T> : Event<T>
    {
        private Event<T> source;
        private IEventListener<T> listener;

        /// <summary>
        /// Firings on the given Event will be forwarded to the current Event
        /// </summary>
        /// <param name="toLoop">Event who's firings will be looped to the current Event</param>
        /// <remarks>Loop can only be called once on an Event. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public Event<T> Loop(Event<T> toLoop)
        {
            if (this.source != null)
            {
                throw new ApplicationException("EventLoop looped more than once");
            }

            this.source = toLoop;
            this.listener = source.Listen(this.CreateFireCallback(), Rank);
            return this;
        }

        /// <summary>
        /// Disposes the current EventLoop
        /// </summary>
        public override void Dispose()
        {
            if (this.listener != null)
            {
                this.listener.Dispose();
                this.listener = null;
            }

            this.source = null;

            base.Dispose();
        }
    }
}
