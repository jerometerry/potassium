namespace Sodium
{
    using System.Linq;

    internal sealed class FilterEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> evt;
        private readonly ILambda1<TA, bool> f;

        public FilterEventSink(Event<TA> evt, ILambda1<TA, bool> f)
        {
            this.evt = evt;
            this.f = f;
        }

        protected internal override TA[] SampleNow()
        {
            var events = evt.SampleNow();
            if (events == null)
            {
                return null;
            }

            var filtered = events.Where(e => f.Apply(e));
            if (!filtered.Any())
            {
                return null;
            }

            return filtered.ToArray();
        }
    }
}