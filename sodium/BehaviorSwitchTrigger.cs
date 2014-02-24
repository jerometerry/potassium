namespace Sodium
{
    internal sealed class BehaviorSwitchTrigger<TA> : ITrigger<Behavior<TA>>
    {
        private readonly EventSink<TA> sink;
        private IListener listener;

        public BehaviorSwitchTrigger(EventSink<TA> sink)
        {
            this.sink = sink;
        }

        ~BehaviorSwitchTrigger()
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

        public void Fire(Transaction t, Behavior<TA> ba)
        {
            // Note: If any switch takes place during a transaction, then the
            // value().listen will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using value().listen, and value() throws away all firings except
            // for the last one. Therefore, anything from the old input behaviour
            // that might have happened during this transaction will be suppressed.
            if (listener != null)
            { 
                listener.Unlisten();
            }

            var evt = ba.Value(t);
            listener = evt.Listen(sink.Node, t, new Trigger<TA>(Handler), false);
        }

        private void Handler(Transaction t3, TA a)
        {
            sink.Send(t3, a);
        }
    }
}