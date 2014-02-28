namespace Sodium
{
    internal sealed class SwitchEventAction<TA> : SodiumItem, ISodiumAction<Event<TA>>
    {
        private Event<TA> evt;
        private ISodiumAction<TA> action;
        private IEventListener<TA> eventListener;
        private Behavior<Event<TA>> bea;

        public SwitchEventAction(Behavior<Event<TA>> bea, Event<TA> evt, Transaction t, ISodiumAction<TA> h)
        {
            this.bea = bea;
            this.evt = evt;
            this.action = h;
            this.eventListener = bea.Sample().Listen(t, h, evt.Rank);
        }

        public void Invoke(Transaction transaction, Event<TA> newEvent)
        {
            transaction.Last(() =>
            {
                if (this.eventListener != null)
                { 
                    this.eventListener.AutoDispose();
                    this.eventListener = null;
                }

                this.eventListener = newEvent.ListenSuppressed(transaction, this.action, evt.Rank);
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.eventListener != null)
                {
                    this.eventListener.AutoDispose();
                    this.eventListener = null;
                }

                if (evt != null)
                {
                    evt.AutoDispose();
                    evt = null;
                }

                if (bea != null)
                {
                    bea.AutoDispose();
                    bea = null;
                }

                action = null;
            }

            base.Dispose(disposing);
        }
    }
}