namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public static class Lifter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bf"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <returns></returns>
        public static IProvider<TB> Apply<TA, TB>(IProvider<Func<TA, TB>> bf, IProvider<TA> source)
        {
            return new UnaryLift<TA, TB>(bf, source);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bf"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <returns></returns>
        public static IProvider<TB> Lift<TA, TB>(Func<TA, TB> bf, IProvider<TA> source)
        {
            return new UnaryLift<TA, TB>(bf, source);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <returns></returns>
        public static IProvider<TC> Lift<TA, TB, TC>(Func<TA, TB, TC> lift, IProvider<TA> a, IProvider<TB> b)
        {
            return new BinaryLift<TA, TB, TC>(lift, a, b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <typeparam name="TD"></typeparam>
        /// <returns></returns>
        public static IProvider<TD> Lift<TA, TB, TC, TD>(Func<TA, TB, TC, TD> lift, IProvider<TA> a, IProvider<TB> b, IProvider<TC> c)
        {
            return new TernaryLift<TA, TB, TC, TD>(lift, a, b, c);
        }
    }
}
