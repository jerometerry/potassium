namespace sodium
{
    internal sealed class Listener<TA> : ListenerBase
    {
        ///
        /// It's essential that we keep the listener alive while the caller holds
        /// the Listener, so that the finalizer doesn't get triggered.
        ///
        private readonly Event<TA> _event;
        private readonly ITransactionHandler<TA> _action;
        private readonly Node _target;

        public Listener(Event<TA> evt, ITransactionHandler<TA> action, Node target)
        {
            _event = evt;
            _action = action;
            _target = target;
        }

        public override void Unlisten()
        {
            lock (Transaction.ListenersLock)
            {
                _event.Actions.Remove(_action);
                _event.Node.UnlinkTo(_target);
            }
        }

        ~Listener()
        {
            Unlisten();
        }
    }
}