namespace Sodium
{
    internal sealed class BehaviorSwitchCallback<TA> : ICallback<Behavior<TA>>
    {
        private readonly EventSink<TA> sink;
        private IListener listener;

        public BehaviorSwitchCallback(EventSink<TA> sink)
        {
            this.sink = sink;
        }

        ~BehaviorSwitchCallback()
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

        public void Invoke(Transaction transaction, Behavior<TA> behavior)
        {
            // Note: If any switch takes place during a transaction, then the
            // Value().Listen will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using Value().Listen, and Value() throws away all firings except
            // for the last one. Therefore, anything from the old input behaviour
            // that might have happened during this transaction will be suppressed.
            if (listener != null)
            { 
                listener.Stop();
            }

            var evt = behavior.Value(transaction);
            listener = evt.ListenUnsuppressed(transaction, new Callback<TA>((t, a) => sink.Fire(t, a)), sink.Rank);
        }
    }
}