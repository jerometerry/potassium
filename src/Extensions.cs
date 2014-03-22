namespace JT.Rx.Net
{
    
    using System;

    public static class Extensions
    {
        /// <summary>
        /// a function 'Bind', that allows us to compose ContinuousBehavior returning functions
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Monad<TB> Bind<TA, TB>(this Monad<TA> a, Func<TA, Monad<TB>> func)
        {
            return func(a.Value);
        }

        public static Monad<TB> Bind<TA, TB>(this IBehavior<TA> source, IBehavior<Func<TA, TB>> bf)
        {
            return new MonadBinder<TA, TB>(source, bf);
        }

        public static Monad<TB> Bind<TA,TB>(this IBehavior<TA> source, Func<TA, TB> bf)
        {
            return new MonadBinder<TA, TB>(source, bf);
        }

        public static Identity<T> ToIdentity<T>(this T value)
        {
            return new Identity<T>(value);
        }

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
