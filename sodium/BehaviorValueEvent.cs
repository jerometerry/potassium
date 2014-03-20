namespace Sodium
{
    /// <summary>
    /// BehaviorValueEvent is an Event that publishes the current value when subscribed to,
    /// and publishes all updates thereafter.
    /// </summary>
    /// <typeparam name="T">The type of values published through the Behavior</typeparam>
    internal sealed class BehaviorValueEvent<T> : SubscribePublishEvent<T>
    {
        private EventBasedBehavior<T> behavior;
        private ISubscription<T> subscription;

        /// <summary>
        /// Constructs a new BehaviorEventSink
        /// </summary>
        /// <param name="behavior">The Behavior to monitor</param>
        /// <param name="transaction">The Transaction to use to create a subscription on the given Behavior</param>
        public BehaviorValueEvent(EventBasedBehavior<T> behavior, Transaction transaction)
        {
            this.behavior = behavior;
            this.CreateLoop(transaction);
        }

        public override T[] SubscriptionFirings()
        {
            // When the ValueEventSink is subscribed to, publish off the current value of the Behavior
            return new[] { this.behavior.Value };
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            this.behavior = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Forward publishings from the behavior to the current BehaviorEventSink
        /// </summary>
        /// <param name="transaction"></param>
        private void CreateLoop(Transaction transaction)
        {
            var source = this.behavior.Source;
            var target = this.CreateSubscriptionPublisher();
            this.subscription = source.CreateSubscription(target, this.Priority, transaction);
        }
    }
}