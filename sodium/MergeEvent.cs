namespace Sodium
{
    using System.Linq;

    internal sealed class MergeEvent<TA> : Event<TA>
    {
        private Event<TA> source1;
        private Event<TA> source2;
        private IEventListener<TA> l1;
        private IEventListener<TA> l2;

        public MergeEvent(Event<TA> source1, Event<TA> source2)
        {
            this.source1 = source1;
            this.source2 = source2;

            var action = new SodiumAction<TA>(this.Fire);
            l1 = source1.Listen(action, this.Rank);
            l2 = source2.Listen(action, this.Rank);
        }

        public override void Dispose()
        {
            if (l1 != null)
            {
                l1.Dispose();
                l1 = null;
            }

            if (l2 != null)
            {
                l2.Dispose();
                l2 = null;
            }

            if (source1 != null)
            {
                source1 = null;
            }

            if (source2 != null)
            {
                source2 = null;
            }

            base.Dispose();
        }

        protected internal override TA[] InitialFirings()
        {
            var firings1 = source1.InitialFirings();
            var firings2 = source2.InitialFirings();

            if (firings1 != null && firings2 != null)
            {
                return firings1.Concat(firings2).ToArray();
            }

            return firings1 ?? firings2;
        }
    }
}