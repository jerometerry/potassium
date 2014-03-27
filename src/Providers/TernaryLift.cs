namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// A TernaryLift lifts a ternary function into an IProvider
    /// </summary>
    /// <typeparam name="T">The type of the first parameter to the ternary function</typeparam>
    /// <typeparam name="TB">The type of the second parameter to the ternary function</typeparam>
    /// <typeparam name="TC">They type of the third parameter to the ternary function</typeparam>
    /// <typeparam name="TD">The return type of the ternary function</typeparam>
    public class TernaryLift<T, TB, TC, TD> : Provider<TD>
    {
        private IProvider<Func<T, TB, TC, TD>> lift;
        private IProvider<T> a;
        private IProvider<TB> b;
        private IProvider<TC> c;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public TernaryLift(Func<T, TB, TC, TD> lift, IProvider<T> a, IProvider<TB> b, IProvider<TC> c)
            : this (new Identity<Func<T, TB, TC, TD>>(lift), a, b, c)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public TernaryLift(IProvider<Func<T, TB, TC, TD>> lift, IProvider<T> a, IProvider<TB> b, IProvider<TC> c)
        {
            this.lift = lift;
            this.a = a;
            this.b = b;
            this.c = c;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override TD Value
        {
            get
            {
                var f = lift.Value;
                var x = a.Value;
                var y = b.Value;
                var z = c.Value;
                return f(x, y, z);
            }
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected override void Dispose(bool disposing)
        {
            lift = null;
            a = null;
            b = null;
            c = null;

            base.Dispose(disposing);
        }
    }
}
