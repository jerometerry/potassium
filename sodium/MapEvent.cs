namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MapEvent<T, TB> : Event<TB>
    {
        private Event<T> source;
        private Func<T, TB> map;
        private IEventListener<T> listener;

        public MapEvent(Event<T> source, Func<T, TB> map)
        {
            this.source = source;
            this.map = map;
            this.listener = source.Listen(new SodiumCallback<T>(this.Fire), this.Rank);
        }

        public void Fire(Transaction trans, T firing)
        {
            Fire(trans, this.map(firing));
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            source = null;
            map = null;

            base.Dispose();
        }

        protected internal override TB[] InitialFirings()
        {
            var firings = source.InitialFirings();
            if (firings == null)
            { 
                return null;
            }

            return firings.Select(e => map(e)).ToArray();
        }
    }
}
