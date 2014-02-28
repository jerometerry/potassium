namespace Sodium
{
    using System;

    public class EventLoop<TA> : Event<TA>
    {
        private Event<TA> loop;
        private IEventListener<TA> loopListener;

        /// <summary>
        /// Firings on the given Event will be forwarded to the current Event
        /// </summary>
        /// <param name="eventToLoop">Event who's firings will be looped to the current Event</param>
        /// <remarks>Loop can only be called once on an Event. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public Event<TA> Loop(Event<TA> eventToLoop)
        {
            AssertNotDisposed();

            if (this.loop != null)
            {
                throw new ApplicationException("EventLoop looped more than once");
            }

            this.loop = eventToLoop;
            var evt = this;
            this.loopListener = eventToLoop.Listen(new SodiumAction<TA>(evt.Fire), evt.Rank);
            return this;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.loopListener != null)
            {
                this.loopListener.AutoDispose();
                this.loopListener = null;
            }

            base.Dispose(disposing);
        }
    }
}
