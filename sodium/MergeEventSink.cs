namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MergeEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> evt1;
        private readonly Event<TA> evt2;

        public MergeEventSink(Event<TA> evt1, Event<TA> evt2)
        {
            this.evt1 = evt1;
            this.evt2 = evt2;
        }

        protected internal override TA[] SampleNow()
        {
            var events1 = evt1.SampleNow();
            var events2 = evt2.SampleNow();

            if (events1 != null && events2 != null)
            {
                return events1.Concat(events2).ToArray();
            }

            return events1 ?? events2;
        }
    }
}