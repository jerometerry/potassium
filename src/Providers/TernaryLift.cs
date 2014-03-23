namespace Potassium.Providers
{
    using System;
    using Potassium.Core;

    /// <summary>
    /// A TernaryMonad lifts a ternary function into a Monad
    /// </summary>
    /// <typeparam name="T">The type of the first parameter to the ternary function</typeparam>
    /// <typeparam name="TB">The type of the second parameter to the ternary function</typeparam>
    /// <typeparam name="TC">They type of the third parameter to the ternary function</typeparam>
    /// <typeparam name="TD">The return type of the ternary function</typeparam>
    public class TernaryLift<T, TB, TC, TD> : Provider<TD>
    {
        private Func<T, TB, TC, TD> lift;
        private IProvider<T> a;
        private IProvider<TB> b;
        private IProvider<TC> c;

        public TernaryLift(Func<T, TB, TC, TD> lift, IProvider<T> a, IProvider<TB> b, IProvider<TC> c)
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
