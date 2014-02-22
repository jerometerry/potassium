namespace Sodium
{
    using System;

    internal sealed class CoalesceEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> evt;
        private readonly ILambda2<TA, TA, TA> f;

        public CoalesceEventSink(Event<TA> evt, ILambda2<TA, TA, TA> f)
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
            
            var e = events[0];
            for (var i = 1; i < events.Length; i++)
            {
                e = f.Apply(e, events[i]);
            }

            return new[] { e };
        }
    }
}