namespace Potassium.Extensions
{
    using System;
    using Potassium.Core;
    using Potassium.Providers;

    /// <summary>
    /// Monad extension methods
    /// </summary>
    public static class ProviderExtensions
    {
        public static Provider<TB> Bind<TA, TB>(this IProvider<TA> source, IProvider<Func<TA, TB>> bf)
        {
            return new Binder<TA, TB>(source, bf);
        }

        public static Provider<TB> Bind<TA,TB>(this IProvider<TA> source, Func<TA, TB> bf)
        {
            return new Binder<TA, TB>(source, bf);
        }

        public static Identity<T> ToIdentity<T>(this T value)
        {
            return new Identity<T>(value);
        }

        public static Provider<TC> Lift<TA, TB, TC>(Func<TA, TB, TC> lift, IProvider<TA> a, IProvider<TB> b)
        {
            return new BinaryLift<TA, TB, TC>(lift, a, b);
        }

        public static Provider<TD> Lift<TA, TB, TC, TD>(Func<TA, TB, TC, TD> lift, IProvider<TA> a, IProvider<TB> b, IProvider<TC> c)
        {
            return new TernaryLift<TA, TB, TC, TD>(lift, a, b, c);
        }

        public static State<TS, TB> Bind<TS, TA, TB>(this State<TS, TA> a, Func<TA, State<TS, TB>> func)
        {
            return new State<TS, TB>(x =>
            {
                var stateContent = a.Computation(x);
                return func(stateContent.Item2).Computation(stateContent.Item1);
            });
        }

        public static State<TS, TA> ToState<TS, TA>(this TA value)
        {
            return new State<TS, TA>(state => Tuple.Create(state, value));
        }

        public static State<TS, TC> SelectManay<TS, TA, TB, TC>(this State<TS, TA> a, Func<TA, State<TS, TB>> func, Func<TA, TB, TC> select)
        {
            return a.Bind(aVal => func(aVal).Bind(bVal => select(aVal, bVal).ToState<TS, TC>()));
        }
    }
}
