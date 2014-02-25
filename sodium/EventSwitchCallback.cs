namespace Sodium
{
    internal sealed class EventSwitchCallback<TA> : ICallback<Event<TA>>
    {
        private readonly EventSink<TA> evt;
        private readonly ICallback<TA> callback;
        private IListener listener;

        public EventSwitchCallback(Behavior<Event<TA>> bea, EventSink<TA> evt, Transaction t, ICallback<TA> h)
        {
            this.evt = evt;
            this.callback = h;
            this.listener = bea.Sample().ListenUnsuppressed(t, h, evt.Rank);
        }

        ~EventSwitchCallback()
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

        public void Invoke(Transaction transaction, Event<TA> data)
        {
            transaction.Last(() =>
            {
                if (listener != null)
                { 
                    listener.Unlisten();
                }

                listener = data.ListenSuppressed(transaction, callback, evt.Rank);
            });
        }
    }
}