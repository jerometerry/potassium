namespace Sodium
{
    internal class SwitchBehaviorEvent<TA> : Event<TA>
    {
        private IEventListener<Behavior<TA>> listener;
        private IEventListener<TA> eventListener;
        private Behavior<Behavior<TA>> bba;
        private Event<TA> valueEvent; 

        public SwitchBehaviorEvent(Behavior<Behavior<TA>> bba)
        {
            this.bba = bba;
            var action = new SodiumAction<Behavior<TA>>(this.Invoke);
            this.listener = bba.Value().Listen(action, this.Rank);
        }

        public void Invoke(Transaction transaction, Behavior<TA> behavior)
        {
            // Note: If any switch takes place during a transaction, then the
            // Value().Listen will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using Value().Listen, and Value() throws away all firings except
            // for the last one. Therefore, anything from the old input behavior
            // that might have happened during this transaction will be suppressed.
            if (this.eventListener != null)
            {
                this.eventListener.Dispose();
                this.eventListener = null;
            }

            if (this.valueEvent != null)
            {
                this.valueEvent.Dispose();
                this.valueEvent = null;
            }

            this.valueEvent = behavior.Value(transaction);
            this.eventListener = valueEvent.Listen(transaction, new SodiumAction<TA>(Fire), Rank);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.listener != null)
                {
                    this.listener.Dispose();
                    this.listener = null;
                }

                if (this.eventListener != null)
                {
                    this.eventListener.Dispose();
                    this.eventListener = null;
                }

                if (this.bba != null)
                {
                    this.bba.Dispose();
                    this.bba = null;
                }

                if (this.valueEvent != null)
                {
                    this.valueEvent.Dispose();
                    this.valueEvent = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
