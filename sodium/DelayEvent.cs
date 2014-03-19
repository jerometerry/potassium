﻿namespace Sodium
{
    internal sealed class DelayEvent<T> : EventPublisher<T>
    {
        private ISubscription<T> subscription;

        public DelayEvent(IObservable<T> source)
        {
            var callback = new Notification<T>((a, t) => t.Low(() => this.Publish(a)));
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
