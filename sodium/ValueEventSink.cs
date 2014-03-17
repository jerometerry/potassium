namespace Sodium
{
    /// <summary>
    /// BehaviorEventSink is an EventSink that fires the Behaviors current value when listened to,
    /// and fires all updates thereafter.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Behavior</typeparam>
    internal sealed class ValueEventSink<T> : InitialFireEventSink<T>
    {
        private IValue<T> valueStream;
        private ISubscription<T> subscription;

        /// <summary>
        /// Constructs a new BehaviorEventSink
        /// </summary>
        /// <param name="valueStream">The Behavior to monitor</param>
        /// <param name="transaction">The Transaction to use to create a subscription on the given Behavior</param>
        public ValueEventSink(IValue<T> valueStream, Transaction transaction)
        {
            this.valueStream = valueStream;
            this.CreateLoop(transaction);
        }

        public override T[] InitialFirings()
        {
            // When the ValueEventSink is subscribed to, fire off the current value of the Behavior
            return new[] { this.valueStream.Value };
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            this.valueStream = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Forward firings from the behavior to the current BehaviorEventSink
        /// </summary>
        /// <param name="transaction"></param>
        private void CreateLoop(Transaction transaction)
        {
            var forward = this.CreateFireCallback();
            this.subscription = this.valueStream.Subscribe(forward, this.Rank, transaction);
        }
    }
}