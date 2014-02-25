namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MapEventSink<TA, TB> : Event<TB>
    {
        private readonly Event<TA> evt;
        private readonly Func<TA, TB> map;

        public MapEventSink(Event<TA> evt, Func<TA, TB> map)
        {
            this.evt = evt;
            this.map = map;
        }

        public void MapAndSend(Transaction trans, TA firing)
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
    }
}
