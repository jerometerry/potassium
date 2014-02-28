namespace Sodium
{
    using System.Linq;

    internal sealed class MergeEvent<TA> : Event<TA>
    {
        private Event<TA> evt1;
        private Event<TA> evt2;
        private IEventListener<TA> l1;
        private IEventListener<TA> l2;

        public MergeEvent(Event<TA> evt1, Event<TA> evt2, bool allowAutoDispose)
            : base(allowAutoDispose)
        {
            this.evt1 = evt1;
            this.evt2 = evt2;

            var action = new SodiumAction<TA>(this.Fire);
            l1 = evt1.Listen(action, this.Rank, true);
            l2 = evt2.Listen(action, this.Rank, true);
        }

        protected internal override TA[] InitialFirings()
        {
            this.AssertNotDisposed();
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
            if (disposing)
            {
                if (l1 != null)
                {
                    l1.AutoDispose();
                    l1 = null;
                }

                if (l2 != null)
                {
                    l2.AutoDispose();
                    l2 = null;
                }

                if (evt1 != null)
                {
                    evt1.AutoDispose();
                    evt1 = null;
                }

                if (evt2 != null)
                {
                    evt2.AutoDispose();
                    evt2 = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}