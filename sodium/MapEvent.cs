namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MapEvent<TA, TB> : Event<TB>
    {
        private readonly Event<TA> evt;
        private readonly Func<TA, TB> map;
        private IEventListener<TA> listener;

        public MapEvent(Event<TA> evt, Func<TA, TB> map)
        {
            this.evt = evt;
            this.map = map;
            this.listener = evt.Listen(new SodiumAction<TA>(this.Fire), this.Rank);
        }

        public void Fire(Transaction trans, TA firing)
        {
            Fire(trans, this.map(firing));
        }

        protected internal override TB[] InitialFirings()
        {
            var firings = evt.InitialFirings();
            if (firings == null)
            { 
                return null;
            }

            return firings.Select(e => map(e)).ToArray();
        }

        protected override void Dispose(bool disposing)
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            base.Dispose(disposing);
        }
    }
}
