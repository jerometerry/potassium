namespace sodium
{
    internal class EventSwitchHandler<TA> : ITransactionHandler<Event<TA>>
    {
        private IListener _currentListener;
        private readonly EventSink<TA> _ev;
        private readonly ITransactionHandler<TA> _h2;

        public EventSwitchHandler(Behavior<Event<TA>> bea, EventSink<TA> ev, Transaction trans1, ITransactionHandler<TA> h2)
        {
            _ev = ev;
            _h2 = h2;
            _currentListener = bea.Sample().Listen(ev.Node, trans1, h2, false);
        }

        public void Run(Transaction trans2, Event<TA> ea)
        {
            trans2.Last(new Runnable(() =>
            {
                if (_currentListener != null)
                    _currentListener.Unlisten();
                _currentListener = ea.Listen(_ev.Node, trans2, _h2, true);
            }));
        }

        ~EventSwitchHandler()
        {
            if (_currentListener != null)
                _currentListener.Unlisten();
        }
    }
}