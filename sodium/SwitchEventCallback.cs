namespace Sodium
{
    internal sealed class SwitchEventCallback<TA> : ICallback<Event<TA>>
    {
        private readonly Event<TA> evt;
        private readonly ICallback<TA> callback;
        private IListener listener;

        public SwitchEventCallback(Behavior<Event<TA>> bea, Event<TA> evt, Transaction t, ICallback<TA> h)
        {
            this.evt = evt;
            this.callback = h;
            this.listener = bea.Sample().ListenUnsuppressed(t, h, evt.Rank);
        }

        ~SwitchEventCallback()
        {
            Close();
        }

        public void Close()
        {
            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }
        }

        public void Invoke(Transaction transaction, Event<TA> newEvent)
        {
            transaction.Last(() =>
            {
                if (listener != null)
                { 
                    listener.Stop();
                }

                listener = newEvent.ListenSuppressed(transaction, callback, this.evt.Rank);
            });
        }
    }
}