namespace Potassium.Providers
{
    using System;
    using Potassium.Extensions;

    /// <summary>
    /// Lifter provides monadic apply and lift operators for IProviders
    /// </summary>
    public static class Lifter
    {
        /// <summary>
        /// Apply an IProvider of values to an IProvider of functions mapping from TA to TB.
        /// </summary>
        /// <param name="fp">IProvider of functions mapping from TA to TB</param>
        /// <param name="a">IProvider of values of type TA</param>
        /// <typeparam name="TA">The input type to the mapping functions</typeparam>
        /// <typeparam name="TB">The return type of the mapping functions</typeparam>
        /// <returns>An IProvider that applies a to f, resulting in an IProvider of type TB</returns>
        public static IProvider<TB> Apply<TA, TB>(IProvider<Func<TA, TB>> fp, IProvider<TA> a)
        {
            return a.Map(fp);
        }

        /// <summary>
        /// Apply an IProvider of values to a function mapping from TA to TB
        /// </summary>
        /// <param name="f">The mapping function from TA to TB</param>
        /// <param name="a">The IProvider of values of type TA</param>
        /// <typeparam name="TA">The input type to the mapping functions</typeparam>
        /// <typeparam name="TB">The return type of the mapping functions</typeparam>
        /// <returns>An IProvider that applies a to f, resulting in an IProvider of type TB</returns>
        public static IProvider<TB> Apply<TA, TB>(Func<TA, TB> f, IProvider<TA> a)
        {
            return a.Map(f);
        }

        /// <summary>
        /// Lift a binary function into an IProvider
        /// </summary>
        /// <param name="lift">The binary function to lift</param>
        /// <param name="a">IProvider of type TA to supply values to the first paramter of the lift function</param>
        /// <param name="b">IProvider of type TB to supply values to the second paramter of the lift function</param>
        /// <typeparam name="TA">The type of values of the first parameter of the lift function</typeparam>
        /// <typeparam name="TB">The type of values of the second parameter of the lift function</typeparam>
        /// <typeparam name="TC">The return type of the lift function</typeparam>
        /// <returns>An IProvider of type TC, who's value is computed by suppling a, and b to the lift function</returns>
        public static IProvider<TC> Lift<TA, TB, TC>(Func<TA, TB, TC> lift, IProvider<TA> a, IProvider<TB> b)
        {
            return new BinaryLift<TA, TB, TC>(lift, a, b);
        }

        /// <summary>
        /// Lift a ternary function into an IProvider
        /// </summary>
        /// <param name="lift">The ternary function to lift</param>
        /// <param name="a">IProvider of type TA to supply values to the first paramter of the lift function</param>
        /// <param name="b">IProvider of type TB to supply values to the second paramter of the lift function</param>
        /// <param name="c">IProvider of type TC to supply values to the third paramter of the lift function</param>
        /// <typeparam name="TA">The type of values of the first parameter of the lift function</typeparam>
        /// <typeparam name="TB">The type of values of the second parameter of the lift function</typeparam>
        /// <typeparam name="TC">The type of values of the third parameter of the lift function</typeparam>
        /// <typeparam name="TD">The return type of the lift function</typeparam>
        /// <returns>An IProvider of type TD, who's value is computed by suppling a, b, and c to the lift function</returns>
        public static IProvider<TD> Lift<TA, TB, TC, TD>(Func<TA, TB, TC, TD> lift, IProvider<TA> a, IProvider<TB> b, IProvider<TC> c)
        {
            return new TernaryLift<TA, TB, TC, TD>(lift, a, b, c);
        }
    }
}
