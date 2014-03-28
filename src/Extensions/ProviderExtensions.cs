namespace Potassium.Extensions
{
    using System;
    using Potassium.Providers;

    /// <summary>
    /// 
    /// </summary>
    public static class ProviderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Identity<T> ToIdentity<T>(this T value)
        {
            return new Identity<T>(value);
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <param name="select"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <returns></returns>
        public static Identity<TC> SelectMany<TA, TB, TC>(this Identity<TA> a, Func<TA, Identity<TB>> func, Func<TA, TB, TC> select)
        {
            return select(a.Value, func(a.Value).Value).ToIdentity();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Maybe<T> ToMaybe<T>(this T value)
        {
            return new Maybe<T>(value);
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <param name="select"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <returns></returns>
        public static Maybe<TC> SelectMany<TA, TB, TC>(this Maybe<TA> a, Func<TA, Maybe<TB>> func, Func<TA, TB, TC> select)
        {
            return a.Bind(aval =>
                    func(aval).Bind(bval =>
                    select(aval, bval).ToMaybe()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="func"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <returns></returns>
        public static Maybe<TB> Select<TA, TB>(this Maybe<TA> a, Func<TA, Maybe<TB>> func)
        {
            return a.Bind(func);
        }
    }
}
