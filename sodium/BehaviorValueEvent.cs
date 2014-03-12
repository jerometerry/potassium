namespace Sodium
{
    internal sealed class BehaviorValueEvent<T> : EventSink<T>
    {
        private Behavior<T> behavior;
        private ISubscription<T> subscription;

        public BehaviorValueEvent(Behavior<T> behavior, Transaction transaction)
        {
            this.behavior = behavior;
            var callback = this.CreateFireCallback();
            this.subscription = behavior.Subscribe(callback, this.Rank, transaction);
        }

        protected internal override T[] InitialFirings()
        {
            return new[] { behavior.Value };
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            behavior = null;

            base.Dispose(disposing);
        }
    }
}