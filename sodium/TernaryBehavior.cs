namespace Sodium
{
    using System;

    public class TernaryBehavior<T, TB, TC, TD> : ContinuousBehavior<TD>
    {
        private Func<T, TB, TC, TD> lift;
        private IBehavior<T> a;
        private IBehavior<TB> b;
        private IBehavior<TC> c;

        public TernaryBehavior(Func<T, TB, TC, TD> lift, IBehavior<T> a, IBehavior<TB> b, IBehavior<TC> c)
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
