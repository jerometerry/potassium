namespace Sodium
{
    using System;

    internal sealed class OnceEventSink<TA> : Event<TA>
    {
        private readonly Event<TA> evt;
        private readonly IListener[] listeners;

        public OnceEventSink(Event<TA> evt, IListener[] listeners)
        {
            this.evt = evt;
            this.listeners = listeners;
        }

        public void Fire(IListener[] la, Transaction t, TA a)
        {
            this.Fire(t, a);
            if (la[0] == null)
            {
                return;
            }

            la[0].Stop();
            la[0] = null;
        }

        protected internal override TA[] InitialFirings()
        {
            var firings = evt.InitialFirings();
            if (firings == null)
            {
                return null;
            }

            var results = firings;
            if (results.Length > 1)
            { 
                results = new[] { firings[0] };
            }

            if (listeners[0] != null)
            {
                listeners[0].Stop();
                listeners[0] = null;
            }

            return results;
        }
    }
}