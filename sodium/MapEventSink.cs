namespace Sodium
{
    using System.Linq;

    internal sealed class MapEventSink<TA, TB> : EventSink<TB>
    {
        private readonly Event<TA> evt;
        private readonly ILambda1<TA, TB> f;

        public MapEventSink(Event<TA> evt, ILambda1<TA, TB> f)
        {
            this.evt = evt;
            this.f = f;
        }

        protected internal override TB[] SampleNow()
        {
            var events = evt.SampleNow();
            if (events == null)
            { 
                return null;
            }

            var results = events.Select(e => f.Apply(e));
            return results.ToArray();
        }
    }
}
