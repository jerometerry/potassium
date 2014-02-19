namespace sodium
{
    sealed class CoalesceHandler<TEvent> : ITransactionHandler<TEvent>
    {
        private readonly IBinaryFunction<TEvent, TEvent, TEvent> _combiningFunction;
        private readonly EventSink<TEvent> _sink;
        private bool _accumulationValid = false;
        private TEvent _accumulation;

        public CoalesceHandler(IBinaryFunction<TEvent, TEvent, TEvent> combiningFunction, EventSink<TEvent> sink)
        {
            _combiningFunction = combiningFunction;
            _sink = sink;
        }

        public void Run(Transaction transaction, TEvent evt)
        {
            if (_accumulationValid)
            {
                _accumulation = _combiningFunction.Apply(_accumulation, evt);
            }
            else
            {
                var action = new Handler<Transaction>(t => 
                {
                    _sink.Send(t, _accumulation);
                    _accumulationValid = false;
                    _accumulation = default(TEvent);
                });
                transaction.Prioritized(_sink.Node, action);
                _accumulation = evt;
                _accumulationValid = true;
            }
        }

        public void Dispose()
        {
        }
    }
}
