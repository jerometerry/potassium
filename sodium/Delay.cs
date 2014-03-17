namespace Sodium
{
    internal sealed class Delay<T> : Sink<T>
    {
        private ISubscription<T> subscription;

        public Delay(IEvent<T> source)
        {
            var callback = new SodiumCallback<T>((a, t) => t.Low(() => this.Fire(a)));
            this.subscription = source.Subscribe(callback, this.Rank);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            base.Dispose(disposing);
        }
    }
}
