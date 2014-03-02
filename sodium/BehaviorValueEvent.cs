namespace Sodium
{
    internal sealed class BehaviorValueEvent<T> : Event<T>
    {
        private Behavior<T> behavior;
        private IEventListener<T> listener;

        public BehaviorValueEvent(Behavior<T> behavior, Transaction transaction)
        {
            this.behavior = behavior;
            var callback = new SodiumCallback<T>(this.Fire);
            listener = behavior.Updates().Listen(transaction, callback, this.Rank);
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            behavior = null;

            base.Dispose();
        }

        protected internal override T[] InitialFirings()
        {
            return new[] { behavior.Sample() };
        }
    }
}