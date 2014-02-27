namespace Sodium
{
    using System;

    internal class BehaviorApplyEvent<TA, TB> : Event<TB>
    {
        public BehaviorApplyEvent(Behavior<Func<TA, TB>> bf, Behavior<TA> ba)
        {
            var h = new BehaviorApplyHandler<TA, TB>(this, bf, ba);
            var functionChanged = new SodiumAction<Func<TA, TB>>((t, f) => h.Run(t));
            var valueChanged = new SodiumAction<TA>((t, a) => h.Run(t));
            bf.Updates().Listen(functionChanged, this.Rank);
            ba.Updates().Listen(valueChanged, this.Rank);
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
            }

            base.Dispose(disposing);
        }
    }
}
