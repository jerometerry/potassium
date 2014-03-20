namespace Sodium
{
    using System;

    public class TernaryBehavior<TA, TB, TC, TD> : DisposableObject, IBehavior<TD>
    {
        private Func<TA, TB, TC, TD> lift;
        private IBehavior<TA> a;
        private IBehavior<TB> b;
        private IBehavior<TC> c;

        public TernaryBehavior(Func<TA, TB, TC, TD> lift, IBehavior<TA> a, IBehavior<TB> b, IBehavior<TC> c)
        {
            this.lift = lift;
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public TD Value
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
