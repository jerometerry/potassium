namespace Potassium.Internal
{
    using Potassium.Core;    

    internal sealed class OnceEvent<T> : FireEvent<T>
    {
        private Observable<T> source;
        private ISubscription<T> subscription;

        public OnceEvent(Observable<T> source)
        {
            this.source = source;
            var observer = new Observer<T>(FireOnce);
            this.subscription = source.Subscribe(observer, this.Priority);
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

            this.Unsubscribe();

            return results;
        }

        protected override void Dispose(bool disposing)
        {
            this.Unsubscribe();
            this.source = null;
            base.Dispose(disposing);
        }

        private void FireOnce(T a, Transaction t)
        {
            this.Fire(a, t);
            this.Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }
        }
    }
}