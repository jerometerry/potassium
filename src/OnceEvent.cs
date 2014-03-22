namespace JT.Rx.Net
{
    

    internal sealed class OnceEvent<T> : SubscribePublishEvent<T>
    {
        private Observable<T> source;
        private ISubscription<T>[] subscriptions;

        public OnceEvent(Observable<T> source)
        {
            this.source = source;

            // This is a bit long-winded but it's efficient because it de-registers
            // the subscription.
            this.subscriptions = new ISubscription<T>[1];
            this.subscriptions[0] = source.Subscribe(new SubscriptionPublisher<T>((a, t) => this.Publish(this.subscriptions, a, t)), this.Priority);
        }

        public override T[] SubscriptionFirings()
        {
            var publishings = GetSubscribeFirings(this.source);
            if (publishings == null)
            {
                return null;
            }

            var results = publishings;
            if (results.Length > 1)
            { 
                results = new[] { publishings[0] };
            }

            if (this.subscriptions[0] != null)
            {
                this.subscriptions[0].Dispose();
                this.subscriptions[0] = null;
            }

            return results;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscriptions[0] != null)
            {
                this.subscriptions[0].Dispose();
                this.subscriptions[0] = null;
            }

            this.source = null;
            this.subscriptions = null;

            base.Dispose(disposing);
        }

        private void Publish(ISubscription<T>[] la, T a, Transaction t)
        {
            this.Publish(a, t);
            if (la[0] == null)
            {
                return;
            }

            la[0].Dispose();
            la[0] = null;
        }
    }
}