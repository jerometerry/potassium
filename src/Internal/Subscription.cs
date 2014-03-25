namespace Potassium.Internal
{
    using Potassium.Core;
    using Potassium.Utilities;

    internal sealed class Subscription<T> : Disposable, ISubscription<T>
    {
        /// <summary>
        /// Constructs a new Subscription
        /// </summary>
        /// <param name="source">The Observable being subscribed to</param>
        /// <param name="observer">The Publisher that contains knowledge of how to notify the caller of updates</param>
        /// <param name="priority">The priority of the subscription.</param>
        /// <remarks>Priority will be Priority.Max for externally triggered subscriptions. Priority will
        /// be the current Priority of an Observable when triggered internally.</remarks>
        public Subscription(Observable<T> source, Observer<T> observer, Priority priority)
        {
            this.Source = source;
            this.Observer = observer;
            this.Priority = priority;
        }

        /// <summary>
        /// The publication source
        /// </summary>
        public Observable<T> Source { get; private set; }

        /// <summary>
        /// Means to notify subscriber of updates via a callback method
        /// </summary>
        public Observer<T> Observer { get; private set; }

        /// <summary>
        /// The Priority of the subscription, used during cancellation to re-prioritize actions in Transactions.
        /// </summary>
        public Priority Priority { get; private set; }

        public void Cancel()
        {
            this.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.Source != null)
            {
                this.Source.CancelSubscription(this);
                this.Source = null;
            }

            this.Observer = null;
            Priority = null;

            base.Dispose(disposing);
        }
    }
}