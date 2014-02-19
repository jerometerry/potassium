namespace sodium
{
    sealed class BehaviorPrioritizedInvoker<TBehavior, TNewBehavior> : IHandler<Transaction>
    {
        public bool Fired = false;
        private readonly EventSink<TNewBehavior> _sink;
        private readonly Behavior<IFunction<TBehavior, TNewBehavior>> _behaviorFunction;
        private readonly Behavior<TBehavior> _behavior;
        private IHandler<Transaction> _action;

        public BehaviorPrioritizedInvoker(
            EventSink<TNewBehavior> sink, 
            Behavior<IFunction<TBehavior, TNewBehavior>> behaviorFunction, 
            Behavior<TBehavior> behavior)
        {
            _sink = sink;
            _behaviorFunction = behaviorFunction;
            _behavior = behavior;
        }

        public void Run(Transaction transaction)
        {
            if (Fired)
                return;

            Fired = true;
            var invoker = this;
            _action = new Handler<Transaction>(t => 
            {
                var mappingFunction = _behaviorFunction.NewValue();
                var behavior = _behavior.NewValue();
                var newBehavior = mappingFunction.Apply(behavior);
                _sink.Send(t, newBehavior);
                invoker.Fired = false;
            });

            transaction.Prioritized(_sink.Node, _action);
        }

        public void Dispose()
        {
            if (_action != null)
            {
                _action.Dispose();
            }
        }
    }
}