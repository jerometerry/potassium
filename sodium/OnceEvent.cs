namespace Sodium
{
    internal sealed class OnceEvent<TA> : Event<TA>
    {
        private Event<TA> evt;
        private IEventListener<TA>[] eventListeners;

        public OnceEvent(Event<TA> source)
        {
            this.evt = source;

            // This is a bit long-winded but it's efficient because it deregisters
            // the listener.
            this.eventListeners = new IEventListener<TA>[1];
            this.eventListeners[0] = source.Listen(new SodiumAction<TA>((t, a) => this.Fire(this.eventListeners, t, a)), this.Rank);
        }

        public override void Dispose()
        {
            if (this.eventListeners[0] != null)
            {
                this.eventListeners[0].Dispose();
                this.eventListeners[0] = null;
            }

            if (evt != null)
            {
                evt = null;
            }

            eventListeners = null;

            base.Dispose();
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
                this.eventListeners[0].Dispose();
                this.eventListeners[0] = null;
            }

            return results;
        }

        private void Fire(IEventListener<TA>[] la, Transaction t, TA a)
        {
            this.Fire(t, a);
            if (la[0] == null)
            {
                return;
            }

            la[0].Dispose();
            la[0] = null;
        }
    }
}