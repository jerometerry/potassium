namespace Sodium
{
    internal sealed class Listener<TA> : IListener
    {
        /// <summary>
        /// It's essential that we keep the listener alive while the caller holds
        /// the Listener, so that the finalizer doesn't get triggered.
        /// </summary>
        private Event<TA> evt;
        private ICallback<TA> action;
        private Node target;

        public Listener(Event<TA> evt, ICallback<TA> action, Node target)
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
            if (evt != null)
            {
                evt.Unlisten(action, target);
                evt = null;
            }

            action = null;
            target = null;
        }
    }
}