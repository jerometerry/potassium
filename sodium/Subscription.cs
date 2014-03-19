namespace Sodium
{
    internal sealed class Subscription<T> : DisposableObject, ISubscription<T>
    {
        public Subscription(IObservable<T> source, IPublisher<T> callback, Rank rank)
        {
            this.Source = source;
            this.Notification = callback;
            this.Rank = rank;
        }

        public IObservable<T> Source { get; private set; }

        public IPublisher<T> Notification { get; private set; }

        public Rank Rank { get; private set; }

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

            this.Notification = null;
            Rank = null;

            base.Dispose(disposing);
        }
    }
}