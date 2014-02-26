namespace Sodium
{
    internal sealed class Listener<TA> : IListener
    {
        private Event<TA> evt;
        private bool disposed;

        public Listener(Event<TA> evt, ICallback<TA> action, Rank rank)
        {
            this.evt = evt;
            this.Action = action;
            this.Rank = rank;
        }

        public ICallback<TA> Action { get; private set; }

        public Rank Rank { get; private set; }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            if (evt != null)
            {
                evt.RemoveListener(this);
                evt = null;
            }

            Action = null;
            Rank = null;
            disposed = true;
        }
    }
}