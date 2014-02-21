namespace sodium
{
    public interface IListener
    {
        void unlisten();

        ///
        /// Combine listeners into one where a single unlisten() invocation will unlisten
        /// both the inputs.
        ///
        IListener append(IListener two);
    }

    public abstract class Listener : IListener
    {
        public Listener() {}

        public virtual void unlisten() {}

        ///
        /// Combine listeners into one where a single unlisten() invocation will unlisten
        /// both the inputs.
        ///
        public IListener append(IListener two) {
            Listener one = this;
            return new DualListener(one, two);
        }
    }

    internal class DualListener : Listener
    {
        private IListener one;
        private IListener two;

        public DualListener(IListener one, IListener two)
        {
            this.one = one;
            this.two = two;
        }

        public override void unlisten()
        {
            this.one.unlisten();
            this.two.unlisten();
        }
    }

    internal sealed class ListenerImplementation<A> : Listener
    {
        ///
        /// It's essential that we keep the listener alive while the caller holds
        /// the Listener, so that the finalizer doesn't get triggered.
        ///
        private readonly Event<A> _event;

        private readonly TransactionHandler<A> action;
        private readonly Node target;

        public ListenerImplementation(Event<A> evt, TransactionHandler<A> action, Node target)
        {
            this._event = evt;
            this.action = action;
            this.target = target;
        }

        public override void unlisten()
        {
            lock (Transaction.listenersLock)
            {
                _event.listeners.Remove(action);
                _event.node.unlinkTo(target);
            }
        }

        ~ListenerImplementation()
        {
            unlisten();
        }
    }

}