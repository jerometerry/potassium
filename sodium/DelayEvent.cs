namespace Sodium
{
    internal class DelayEvent<T> : EventSink<T>
    {
        private ISubscription<T> subscription;

        public DelayEvent(Event<T> source)
        {
            var callback = new ActionCallback<T>((a, t) => t.Low(() => this.Fire(a)));
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
