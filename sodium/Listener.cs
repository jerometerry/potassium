namespace sodium
{
    using System;

    sealed class Listener<TEvent> : ListenerBase, IDisposable
    {
        /// <summary>
        /// It's essential that we keep the listener alive while the caller holds
        /// the Listener, so that the finalizer doesn't get triggered.
        /// </summary>
        private readonly Event<TEvent> _event;
        private readonly ITransactionHandler<TEvent> _action;
        private readonly Node _target;

        public Listener(Event<TEvent> evt, ITransactionHandler<TEvent> action, Node target)
        {
            _event = evt;
            _action = action;
            _target = target;
        }

        public override void Unlisten()
        {
            lock (Transaction.ListenersLock)
            {
                _event.RemoveAction(_action);
                _event.Node.UnlinkTo(_target);
            }
        }

        public override void Dispose()
        {
            Unlisten();
        }
    }
}
