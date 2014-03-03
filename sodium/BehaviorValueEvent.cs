namespace Sodium
{
    internal sealed class BehaviorValueEvent<T> : EventSink<T>
    {
        private Behavior<T> behavior;
        private IEventListener<T> listener;

        public BehaviorValueEvent(Behavior<T> behavior, Transaction transaction)
        {
            this.behavior = behavior;
            var callback = this.CreateFireCallback();
            listener = behavior.Listen(callback, this.Rank, transaction);
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
            return new[] { behavior.Value };
        }
    }
}