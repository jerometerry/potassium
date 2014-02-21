namespace sodium
{
    internal class BehaviorSwitchHandler<A> : ITransactionHandler<Behavior<A>>
    {
        private IListener _currentListener;
        private readonly EventSink<A> _sink;

        public BehaviorSwitchHandler(EventSink<A> sink)
        {
            _sink = sink;
        }

        public void Run(Transaction trans2, Behavior<A> ba)
        {
            // Note: If any switch takes place during a transaction, then the
            // value().listen will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using value().listen, and value() throws away all firings except
            // for the last one. Therefore, anything from the old input behaviour
            // that might have happened during this transaction will be suppressed.
            if (_currentListener != null)
                _currentListener.unlisten();

            var ev = ba.Value(trans2);
            _currentListener = ev.Listen(_sink.Node, trans2, new TransactionHandler<A>(Handler), false);
        }

        private void Handler(Transaction t3, A a)
        {
            _sink.send(t3, a);
        }

        ~BehaviorSwitchHandler()
        {
            if (_currentListener != null)
                _currentListener.unlisten();
        }
    }
}