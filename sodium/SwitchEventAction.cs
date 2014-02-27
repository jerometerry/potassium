namespace Sodium
{
    using System;

    internal sealed class SwitchEventAction<TA> : ISodiumAction<Event<TA>>, IDisposable
    {
        private readonly Event<TA> evt;
        private readonly ISodiumAction<TA> action;
        private IEventListener<TA> eventListener;
        private bool disposed;

        public SwitchEventAction(Behavior<Event<TA>> bea, Event<TA> evt, Transaction t, ISodiumAction<TA> h)
        {
            this.evt = evt;
            this.action = h;
            this.eventListener = bea.Sample().Listen(t, h, evt.Rank);
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

        public void Invoke(Transaction transaction, Event<TA> newEvent)
        {
            transaction.Last(() =>
            {
                if (this.eventListener != null)
                { 
                    this.eventListener.Dispose();
                    this.eventListener = null;
                }

                this.eventListener = newEvent.ListenSuppressed(transaction, this.action, evt.Rank);
            });
        }
    }
}