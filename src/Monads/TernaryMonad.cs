namespace JT.Rx.Net.Monads
{
    
    using System;

    /// <summary>
    /// TernaryBehavior is a continuous Behavior that lifts a ternary function into a Behavior.
    /// The value of the TernaryBehavior is computed by calling the ternary function, where
    /// the parameters are the current values of Behaviors.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter to the ternary function</typeparam>
    /// <typeparam name="TB">The type of the second parameter to the ternary function</typeparam>
    /// <typeparam name="TC">They type of the third parameter to the ternary function</typeparam>
    /// <typeparam name="TD">The return type of the ternary function</typeparam>
    public class TernaryMonad<T, TB, TC, TD> : Monad<TD>
    {
        private Func<T, TB, TC, TD> lift;
        private IValueSource<T> a;
        private IValueSource<TB> b;
        private IValueSource<TC> c;

        public TernaryMonad(Func<T, TB, TC, TD> lift, IValueSource<T> a, IValueSource<TB> b, IValueSource<TC> c)
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
