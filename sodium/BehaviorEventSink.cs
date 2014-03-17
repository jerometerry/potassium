namespace Sodium
{
    /// <summary>
    /// BehaviorEventSink is an EventSink that fires the Behaviors current value when listened to,
    /// and fires all updates thereafter.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Behavior</typeparam>
    internal sealed class BehaviorEventSink<T> : InitialFireEventSink<T>
    {
        private IBehavior<T> behavior;
        private ISubscription<T> subscription;

        /// <summary>
        /// Constructs a new BehaviorEventSink
        /// </summary>
        /// <param name="behavior">The Behavior to monitor</param>
        /// <param name="transaction">The Transaction to use to create a subscription on the given Behavior</param>
        public BehaviorEventSink(IBehavior<T> behavior, Transaction transaction)
        {
            this.behavior = behavior;
            this.CreateLoop(transaction);
        }

        protected internal override T[] InitialFirings()
        {
            // When the BehaviorEventSink is subscribed to, fire off the current value of the Behavior
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

        /// <summary>
        /// Forward firings from the behavior to the current BehaviorEventSink
        /// </summary>
        /// <param name="transaction"></param>
        private void CreateLoop(Transaction transaction)
        {
            var forward = this.CreateFireCallback();
            this.subscription = behavior.Subscribe(forward, this.Rank, transaction);
        }
    }
}