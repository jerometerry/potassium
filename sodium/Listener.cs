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
        private Rank rank;

        public Listener(Event<TA> evt, ICallback<TA> action, Rank rank)
        {
            this.evt = evt;
            this.action = action;
            this.rank = rank;
        }

        public void Stop()
        {
            if (evt != null)
            {
                evt.Unlisten(action, this.rank);
                evt = null;
            }

            action = null;
            this.rank = null;
        }
    }
}