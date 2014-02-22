namespace Sodium
{
    internal sealed class EventSwitchHandler<TA> : ITransactionHandler<Event<TA>>
    {
        private IListener listener;
        private readonly EventSink<TA> evt;
        private readonly ITransactionHandler<TA> handler;

        public EventSwitchHandler(Behavior<Event<TA>> bea, EventSink<TA> evt, Transaction t, ITransactionHandler<TA> h)
        {
            this.evt = evt;
            this.handler = h;
            this.listener = bea.Sample().Listen(evt.Node, t, h, false);
        }

        public void Run(Transaction t, Event<TA> e)
        {
            t.Last(new Runnable(() =>
            {
                if (listener != null)
                { 
                    listener.Unlisten();
                }

                listener = e.Listen(evt.Node, t, handler, true);
            }));
        }

        ~EventSwitchHandler()
        {
            if (listener != null)
            { 
                listener.Unlisten();
            }
        }
    }
}