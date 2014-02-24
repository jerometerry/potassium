namespace Sodium
{
    internal sealed class EventSwitchTrigger<TA> : ITrigger<Event<TA>>
    {
        private readonly EventSink<TA> evt;
        private readonly ITrigger<TA> trigger;
        private IListener listener;

        public EventSwitchTrigger(Behavior<Event<TA>> bea, EventSink<TA> evt, Transaction t, ITrigger<TA> h)
        {
            this.evt = evt;
            this.trigger = h;
            this.listener = bea.Sample().Listen(evt.Node, t, h, false);
        }

        ~EventSwitchTrigger()
        {
            Close();
        }

        public void Close()
        {
            if (listener != null)
            {
                listener.Unlisten();
                listener = null;
            }
        }

        public void Fire(Transaction t, Event<TA> e)
        {
            t.Last(() =>
            {
                if (listener != null)
                { 
                    listener.Unlisten();
                }

                listener = e.Listen(evt.Node, t, this.trigger, true);
            });
        }
    }
}