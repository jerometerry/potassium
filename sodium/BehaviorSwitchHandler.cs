namespace sodium
{
    internal class BehaviorSwitchHandler<TA> : ITransactionHandler<Behavior<TA>>
    {
        private IListener _currentListener;
        private readonly EventSink<TA> _sink;

        public BehaviorSwitchHandler(EventSink<TA> sink)
        {
            _sink = sink;
        }

        public void Run(Transaction t2, Behavior<TA> ba)
        {
            // Note: If any switch takes place during a transaction, then the
            // value().listen will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using value().listen, and value() throws away all firings except
            // for the last one. Therefore, anything from the old input behaviour
            // that might have happened during this transaction will be suppressed.
            if (_currentListener != null)
            { 
                _currentListener.Unlisten();
            }

            var ev = ba.Value(t2);
            _currentListener = ev.Listen(_sink.Node, t2, new TransactionHandler<TA>(Handler), false);
        }

        private void Handler(Transaction t3, TA a)
        {
            _sink.Send(t3, a);
        }

        ~BehaviorSwitchHandler()
        {
            if (_currentListener != null)
            { 
                _currentListener.Unlisten();
            }
        }
    }
}