namespace Sodium
{
    internal sealed class OnceEvent<TA> : Event<TA>
    {
        private Event<TA> evt;
        private IEventListener<TA>[] eventListeners;
        private IEventListener<TA> eventListener;

        public OnceEvent(Event<TA> evt)
        {
            this.evt = evt;

            // This is a bit long-winded but it's efficient because it deregisters
            // the listener.
            this.eventListeners = new IEventListener<TA>[1];
            this.eventListeners[0] = evt.Listen(new SodiumAction<TA>((t, a) => this.Fire(this.eventListeners, t, a)), this.Rank);
            this.eventListener = this.eventListeners[0];
        }

        public void Fire(IEventListener<TA>[] la, Transaction t, TA a)
        {
            this.Fire(t, a);
            if (la[0] == null)
            {
                return;
            }

            la[0].Close();
            la[0] = null;
        }

        protected internal override TA[] InitialFirings()
        {
            var firings = evt.InitialFirings();
            if (firings == null)
            {
                return null;
            }

            var results = firings;
            if (results.Length > 1)
            { 
                results = new[] { firings[0] };
            }

            if (this.eventListeners[0] != null)
            {
                this.eventListeners[0].Close();
                this.eventListeners[0] = null;
            }

            if (this.eventListener != null)
            {
                this.eventListener.Close();
                this.eventListener = null;
            }

            return results;
        }

        public override void Close()
        {
            if (this.eventListeners[0] != null)
            {
                this.eventListeners[0].Close();
                this.eventListeners[0] = null;
            }

            if (this.eventListener != null)
            {
                this.eventListener.Close();
                this.eventListener = null;
            }

            if (evt != null)
            {
                evt = null;
            }

            eventListeners = null;

            base.Close();
        }
    }
}