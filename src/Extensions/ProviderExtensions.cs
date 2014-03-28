namespace Potassium.Extensions
{
    using System;
    using Potassium.Providers;

    /// <summary>
    /// Provider extension methods
    /// </summary>
    public static class ProviderExtensions
    {
        /// <summary>
        /// Lift the given value into an Identity
        /// </summary>
        /// <param name="value">The value to lift</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>The Identity containing the lifted value</returns>
        public static Identity<T> ToIdentity<T>(this T value)
        {
            return new Identity<T>(value);
        }

        /// <summary>
        /// Monadic bind method for Identity
        /// </summary>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <returns></returns>
        public static Identity<TB> Bind<TA, TB>(this Identity<TA> a, Func<TA, Identity<TB>> func)
        {
            return func(a.Value);
        }

        /// <summary>
        /// The monadic map method for Identity
        /// </summary>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <returns></returns>
        public static Identity<TB> Map<TA, TB>(this Identity<TA> a, Func<TA, Identity<TB>> func)
        {
            return a.Bind(func);
        }

        /// <summary>
        /// Monadic flap map method for Identity
        /// </summary>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <param name="select"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <returns></returns>
        public static Identity<TC> FlapMap<TA, TB, TC>(this Identity<TA> a, Func<TA, Identity<TB>> func, Func<TA, TB, TC> select)
        {
            return select(a.Value, func(a.Value).Value).ToIdentity();
        }

        /// <summary>
        /// Lift the given value into a Maybe
        /// </summary>
        /// <param name="value">The value to lift</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>The Maybe containing the lifed value</returns>
        public static Maybe<T> ToMaybe<T>(this T value)
        {
            return new Maybe<T>(value);
        }

        /// <summary>
        /// Monadic bind method for Maybe
        /// </summary>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <returns></returns>
        public static Maybe<TB> Bind<TA, TB>(this Maybe<TA> a, Func<TA, Maybe<TB>> func)
        {
            return a.HasValue ?
                func(a.Value) :
                Maybe<TB>.Nothing;
        }

        /// <summary>
        /// The monadic map method for Maybe
        /// </summary>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <returns></returns>
        public static Maybe<TB> Map<TA, TB>(this Maybe<TA> a, Func<TA, Maybe<TB>> func)
        {
            return a.Bind(func);
        }

        /// <summary>
        /// Monadic flap map method for Maybe
        /// </summary>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <param name="select"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <returns></returns>
        public static Maybe<TC> FlapMap<TA, TB, TC>(this Maybe<TA> a, Func<TA, Maybe<TB>> func, Func<TA, TB, TC> select)
        {
            return a.Bind(aval =>
                    func(aval).Bind(bval =>
                    select(aval, bval).ToMaybe()));
        }
    }
}
