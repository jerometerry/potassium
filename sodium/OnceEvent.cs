namespace Sodium
{
    internal sealed class OnceEvent<T> : Event<T>
    {
        private Event<T> source;
        private IEventListener<T>[] eventListeners;

        public OnceEvent(Event<T> source)
        {
            this.source = source;

            // This is a bit long-winded but it's efficient because it de-registers
            // the listener.
            this.eventListeners = new IEventListener<T>[1];
            this.eventListeners[0] = source.Listen(new SodiumAction<T>((t, a) => this.Fire(this.eventListeners, t, a)), this.Rank);
        }

        public override void Dispose()
        {
            if (this.eventListeners[0] != null)
            {
                this.eventListeners[0].Dispose();
                this.eventListeners[0] = null;
            }

            this.source = null;
            eventListeners = null;

            base.Dispose();
        }

        protected internal override T[] InitialFirings()
        {
            var firings = this.source.InitialFirings();
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

        private void Fire(IEventListener<T>[] la, Transaction t, T a)
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