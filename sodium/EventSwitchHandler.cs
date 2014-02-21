namespace sodium
{
    internal class EventSwitchHandler<TA> : ITransactionHandler<Event<TA>>
    {
        private IListener _currentListener;
        private readonly EventSink<TA> _ev;
        private readonly ITransactionHandler<TA> _h2;

        public EventSwitchHandler(Behavior<Event<TA>> bea, EventSink<TA> ev, Transaction t1, ITransactionHandler<TA> h2)
        {
            _ev = ev;
            _h2 = h2;
            _currentListener = bea.Sample().Listen(ev.Node, t1, h2, false);
        }

        public void Run(Transaction t2, Event<TA> ea)
        {
            t2.Last(new Runnable(() =>
            {
                if (_currentListener != null)
                    _currentListener.Unlisten();
                _currentListener = ea.Listen(_ev.Node, t2, _h2, true);
            }));
        }

        ~EventSwitchHandler()
        {
            if (_currentListener != null)
                _currentListener.Unlisten();
        }
    }
}