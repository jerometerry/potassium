namespace Sodium
{
    using System;

    internal sealed class SwitchBehaviorAction<TA> : ISodiumAction<Behavior<TA>>, IDisposable
    {
        private readonly Event<TA> sink;
        private IEventListener<TA> eventListener;
        private bool disposed;

        public SwitchBehaviorAction(Event<TA> sink)
        {
            this.sink = sink;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            if (this.eventListener != null)
            {
                this.eventListener.Dispose();
                this.eventListener = null;
            }

            disposed = true;
        }

        public void Invoke(Transaction transaction, Behavior<TA> behavior)
        {
            // Note: If any switch takes place during a transaction, then the
            // Value().Listen will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using Value().Listen, and Value() throws away all firings except
            // for the last one. Therefore, anything from the old input behaviour
            // that might have happened during this transaction will be suppressed.
            if (this.eventListener != null)
            { 
                this.eventListener.Dispose();
            }

            var evt = behavior.Value(transaction);
            this.eventListener = evt.Listen(transaction, new SodiumAction<TA>((t, a) => sink.Fire(t, a)), sink.Rank);
        }
    }
}