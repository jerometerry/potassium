namespace Sodium
{
    internal sealed class Once<T> : FireOnSubscribeEvent<T>
    {
        private IObservable<T> source;
        private ISubscription<T>[] subscriptions;

        public Once(IObservable<T> source)
        {
            this.source = source;

            // This is a bit long-winded but it's efficient because it de-registers
            // the subscription.
            this.subscriptions = new ISubscription<T>[1];
            this.subscriptions[0] = source.Subscribe(new SodiumCallback<T>((a, t) => this.Fire(this.subscriptions, a, t)), this.Rank);
        }

        public override T[] SubscriptionFirings()
        {
            var firings = GetSubscribeFirings(this.source);
            if (firings == null)
            {
                return null;
            }

            var results = firings;
            if (results.Length > 1)
            { 
                results = new[] { firings[0] };
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

        private void Fire(ISubscription<T>[] la, T a, Transaction t)
        {
            this.Fire(a, t);
            if (la[0] == null)
            {
                return;
            }

            la[0].Dispose();
            la[0] = null;
        }
    }
}