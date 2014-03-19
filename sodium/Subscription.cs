namespace Sodium
{
    internal sealed class Subscription<T> : DisposableObject, ISubscription<T>
    {
        public Subscription(Observable<T> source, IPublisher<T> publisher, Rank rank)
        {
            this.Source = source;
            this.Publisher = publisher;
            this.Rank = rank;
        }

        public Observable<T> Source { get; private set; }

        public IPublisher<T> Publisher { get; private set; }

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

            this.Publisher = null;
            Rank = null;

            base.Dispose(disposing);
        }
    }
}