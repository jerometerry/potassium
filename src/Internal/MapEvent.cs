namespace Potassium.Internal
{
    using System;
    using System.Linq;
    using Potassium.Core;

    internal sealed class MapEvent<T, TB> : FireEvent<TB>
    {
        private Observable<T> source;
        private Func<T, TB> map;
        private ISubscription<T> subscription;

        public MapEvent(Observable<T> source, Func<T, TB> map)
        {
            this.source = source;
            this.map = map;
            this.subscription = source.Subscribe(new Observer<T>(this.Fire), this.Priority);
        }

        public void Fire(T firing, Transaction trans)
        {
            var v = this.map(firing);
            this.Fire(v, trans);
        }

        public override TB[] SubscriptionFirings()
        {
            var firings = GetSubscribeFirings(source);
            if (firings == null)
            { 
                return null;
            }

            return firings.Select(e => map(e)).ToArray();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            source = null;
            map = null;

            base.Dispose(disposing);
        }
    }
}
