namespace Sodium
{
    internal sealed class Listener<TA> : IListener
    {
        /// <summary>
        /// It's essential that we keep the listener alive while the caller holds
        /// the Listener, so that the finalizer doesn't get triggered.
        /// </summary>
        private Event<TA> evt;
        private ITransactionHandler<TA> action;
        private Node target;

        private bool disposed;

        public Listener(Event<TA> evt, ITransactionHandler<TA> action, Node target)
        {
            this.evt = evt;
            this.action = action;
            this.target = target;
        }

        ~Listener()
        {
            Unlisten();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                this.Unlisten();
                evt = null;
                action = null;
                target = null;
                disposed = true;
            }
        }

        public void Unlisten()
        {
            evt.Unlisten(action, target);
        }
    }
}