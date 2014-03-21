namespace JT.Rx.Net.Continuous
{
    using JT.Rx.Net.Core;
    using System;

    public static class Extensions
    {
        public static Monad<TC> Lift<TA, TB, TC>(Func<TA, TB, TC> lift, IBehavior<TA> a, IBehavior<TB> b)
        {
            return new BinaryMonad<TA, TB, TC>(lift, a, b);
        }

        public static Monad<TD> Lift<TA, TB, TC, TD>(Func<TA, TB, TC, TD> lift, IBehavior<TA> a, IBehavior<TB> b, IBehavior<TC> c)
        {
            return new TernaryMonad<TA, TB, TC, TD>(lift, a, b, c);
        }
    }
}
