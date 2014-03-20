namespace Sodium
{
    using System;

    public class TernaryBehavior<TA, TB, TC, TD> : Behavior<TD>
    {
        private Func<TA, TB, TC, TD> lift;
        private Behavior<TA> a;
        private Behavior<TB> b;
        private Behavior<TC> c;

        public TernaryBehavior(Func<TA, TB, TC, TD> lift, Behavior<TA> a, Behavior<TB> b, Behavior<TC> c)
        {
            this.lift = lift;
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override TD Value
        {
            get
            {
                return lift(a.Value, b.Value, c.Value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lift = null;
                a = null;
                b = null;
                c = null;
            }

            base.Dispose(disposing);
        }
    }
}
