namespace sodium
{
    internal sealed class Listener<A> : ListenerBase
    {
        ///
        /// It's essential that we keep the listener alive while the caller holds
        /// the Listener, so that the finalizer doesn't get triggered.
        ///
        private readonly Event<A> _event;

        private readonly TransactionHandler<A> action;
        private readonly Node target;

        public Listener(Event<A> evt, TransactionHandler<A> action, Node target)
        {
            this._event = evt;
            this.action = action;
            this.target = target;
        }

        public override void unlisten()
        {
            lock (Transaction.ListenersLock)
            {
                _event.listeners.Remove(action);
                _event.node.unlinkTo(target);
            }
        }

        ~Listener()
        {
            unlisten();
        }
    }
}