namespace Sodium
{
    internal sealed class OnceEvent<TA> : Event<TA>
    {
        private readonly Event<TA> evt;
        private readonly IListener<TA>[] listeners;
        private IListener<TA> listener;

        public OnceEvent(Event<TA> evt)
        {
            this.evt = evt;

            // This is a bit long-winded but it's efficient because it deregisters
            // the listener.
            this.listeners = new IListener<TA>[1];
            this.listeners[0] = evt.Listen(new Callback<TA>((t, a) => this.Fire(this.listeners, t, a)), this.Rank);
            listener = this.listeners[0];
        }

        public void Fire(IListener<TA>[] la, Transaction t, TA a)
        {
            this.Fire(t, a);
            if (la[0] == null)
            {
                return;
            }

            la[0].Dispose();
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

            if (listeners[0] != null)
            {
                listeners[0].Dispose();
                listeners[0] = null;
            }

            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            return results;
        }
    }
}