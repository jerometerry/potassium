namespace Sodium
{
    internal sealed class Listener<TA> : IListener
    {
        /// <summary>
        /// It's essential that we keep the listener alive while the caller holds
        /// the Listener, so that the finalizer doesn't get triggered.
        /// </summary>
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

        public void Unlisten()
        {
            evt.Unlisten(action, target);
        }
    }
}