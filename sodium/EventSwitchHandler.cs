namespace Sodium
{
    internal sealed class EventSwitchHandler<TA> : IHandler<Event<TA>>
    {
        private readonly EventSink<TA> evt;
        private readonly IHandler<TA> handler;
        private IListener listener;

        public EventSwitchHandler(Behavior<Event<TA>> bea, EventSink<TA> evt, Transaction t, IHandler<TA> h)
        {
            this.evt = evt;
            this.handler = h;
            this.listener = bea.Sample().Listen(evt.Node, t, h, false);
        }

        ~EventSwitchHandler()
        {
            if (listener != null)
            {
                listener.Unlisten();
            }
        }

        public void Run(Transaction t, Event<TA> e)
        {
            t.Last(() =>
            {
                if (listener != null)
                { 
                    listener.Unlisten();
                }

                listener = e.Listen(evt.Node, t, handler, true);
            });
        }
    }
}