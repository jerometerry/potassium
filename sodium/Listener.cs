namespace Sodium
{
    internal sealed class Listener<TA> : ListenerBase
    {
        ///
        /// It's essential that we keep the listener alive while the caller holds
        /// the Listener, so that the finalizer doesn't get triggered.
        ///
        private readonly Event<TA> evt;
        private readonly ITransactionHandler<TA> action;
        private readonly Node target;

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

        public override void Unlisten()
        {
            lock (Transaction.ListenersLock)
            {
                evt.RemoveAction(action);
                evt.Node.UnlinkTo(target);
            }
        }
    }
}