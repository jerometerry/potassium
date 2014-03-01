namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MapEvent<TA, TB> : Event<TB>
    {
        private Event<TA> source;
        private Func<TA, TB> map;
        private IEventListener<TA> listener;

        public MapEvent(Event<TA> source, Func<TA, TB> map)
        {
            this.source = source;
            this.map = map;
            this.listener = source.Listen(new SodiumAction<TA>(this.Fire), this.Rank);
        }

        public void Fire(Transaction trans, TA firing)
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

            if (source != null)
            {
                source = null;
            }

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
