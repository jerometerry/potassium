namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MergeEvent<TA> : Event<TA>
    {
        private readonly Event<TA> evt1;
        private readonly Event<TA> evt2;

        public MergeEvent(Event<TA> evt1, Event<TA> evt2)
        {
            this.evt1 = evt1;
            this.evt2 = evt2;
        }

        protected internal override TA[] InitialFirings()
        {
            var firings1 = evt1.InitialFirings();
            var firings2 = evt2.InitialFirings();

            if (firings1 != null && firings2 != null)
            {
                return firings1.Concat(firings2).ToArray();
            }

            return firings1 ?? firings2;
        }
    }
}