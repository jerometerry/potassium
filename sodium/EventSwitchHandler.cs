namespace sodium
{
    internal class EventSwitchHandler<A> : ITransactionHandler<Event<A>>
    {
        private IListener _currentListener;
        private readonly EventSink<A> _ev;
        private Transaction _trans1;
        private readonly ITransactionHandler<A> _h2;

        public EventSwitchHandler(Behavior<Event<A>> bea, EventSink<A> ev, Transaction trans1, ITransactionHandler<A> h2)
        {
            _ev = ev;
            _trans1 = trans1;
            _h2 = h2;
            _currentListener = bea.Sample().Listen(ev.Node, trans1, h2, false);
        }

        public void Run(Transaction trans2, Event<A> ea)
        {
            trans2.Last(new Runnable(() =>
            {
                if (_currentListener != null)
                    _currentListener.unlisten();
                _currentListener = ea.Listen(_ev.Node, trans2, _h2, true);
            }));
        }

        ~EventSwitchHandler()
        {
            if (_currentListener != null)
                _currentListener.unlisten();
        }
    }
}