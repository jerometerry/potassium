namespace Sodium
{
    using System;

    internal class BehaviorApplyEvent<TA, TB> : Event<TB>
    {
        private Behavior<Func<TA, TB>> bf;
        private Behavior<TA> ba;
        private IEventListener<Func<TA, TB>> l1;
        private IEventListener<TA> l2;
        private bool fired;

        public BehaviorApplyEvent(Behavior<Func<TA, TB>> bf, Behavior<TA> ba)
        {
            this.bf = bf;
            this.ba = ba;

            var functionChanged = new SodiumAction<Func<TA, TB>>((t, f) => this.Run(t));
            var valueChanged = new SodiumAction<TA>((t, a) => this.Run(t));

            l1 = bf.Updates().Listen(functionChanged, this.Rank);
            l2 = ba.Updates().Listen(valueChanged, this.Rank);

            var map = bf.Sample();
            var valA = ba.Sample();
            var valB = map(valA);
            this.Behavior = this.Hold(valB);
        }

        public Behavior<TB> Behavior { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Behavior != null)
                {
                    this.Behavior.Dispose();
                    this.Behavior = null;
                }

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

                bf = null;
                ba = null;
            }

            base.Dispose(disposing);
        }

        private void Run(Transaction t1)
        {
            if (fired)
            {
                return;
            }

            fired = true;
            t1.Prioritize(Fire, this.Rank);
        }

        private void Fire(Transaction t)
        {
            var map = bf.NewValue();
            var a = ba.NewValue();
            var b = map(a);
            this.Fire(t, b);
            fired = false;
        }
    }
}
