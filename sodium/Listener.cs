namespace Sodium
{
    internal sealed class Listener<TA> : IListener<TA>
    {
        private bool disposed;

        public Listener(Event<TA> evt, ICallback<TA> action, Rank rank)
        {
            this.Event = evt;
            this.Action = action;
            this.Rank = rank;
        }

        public ICallback<TA> Action { get; private set; }

        public Rank Rank { get; private set; }

        public Event<TA> Event { get; private set; }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            if (this.Event != null)
            {
                this.Event.RemoveListener(this);
                this.Event = null;
            }

            Action = null;
            Rank = null;
            disposed = true;
        }
    }
}