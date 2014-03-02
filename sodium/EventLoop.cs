namespace Sodium
{
    using System;

    public class EventLoop<T> : Event<T>
    {
        private Event<T> loop;
        private IEventListener<T> loopListener;

        public EventLoop()
        {
        }

        /// <summary>
        /// Firings on the given Event will be forwarded to the current Event
        /// </summary>
        /// <param name="eventToLoop">Event who's firings will be looped to the current Event</param>
        /// <remarks>Loop can only be called once on an Event. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public Event<T> Loop(Event<T> eventToLoop)
        {
            if (this.loop != null)
            {
                throw new ApplicationException("EventLoop looped more than once");
            }

            this.loop = eventToLoop;
            var evt = this;
            this.loopListener = eventToLoop.Listen(new SodiumAction<T>(evt.Fire), evt.Rank);
            return this;
        }

        public override void Dispose()
        {
            if (this.loopListener != null)
            {
                this.loopListener.Dispose();
                this.loopListener = null;
            }

            if (this.loop != null)
            {
                this.loop = null;
            }

            base.Dispose();
        }
    }
}
