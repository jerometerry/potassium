namespace Sodium
{
    using System.Linq;

    internal sealed class MergeEvent<TA> : Event<TA>
    {
        private readonly Event<TA> evt1;
        private readonly Event<TA> evt2;
        private IEventListener<TA> l1;
        private IEventListener<TA> l2;

        public MergeEvent(Event<TA> evt1, Event<TA> evt2)
        {
            this.evt1 = evt1;
            this.evt2 = evt2;

            var action = new SodiumAction<TA>(this.Fire);
            l1 = evt1.Listen(action, this.Rank);
            l2 = evt2.Listen(action, this.Rank);
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

        protected override void Dispose(bool disposing)
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

            base.Dispose(disposing);
        }
    }
}