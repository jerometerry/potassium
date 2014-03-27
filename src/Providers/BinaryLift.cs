namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// A BinaryLift lifts a binary function into an IProvider
    /// </summary>
    /// <typeparam name="T">The type of first parameter to the lift function</typeparam>
    /// <typeparam name="TB">The type of the second parameter of the lift function</typeparam>
    /// <typeparam name="TC">The return type of the lift function</typeparam>
    public class BinaryLift<T, TB, TC> : Provider<TC>
    {
        private IProvider<Func<T, TB, TC>> lift;
        private IProvider<T> a;
        private IProvider<TB> b;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public BinaryLift(Func<T, TB, TC> lift, IProvider<T> a, IProvider<TB> b)
            : this(new Identity<Func<T, TB, TC>>(lift), a, b)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public BinaryLift(IProvider<Func<T, TB, TC>> lift, IProvider<T> a, IProvider<TB> b)
        {
            this.lift = lift;
            this.a = a;
            this.b = b;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override TC Value
        {
            get
            {
                var f = lift.Value;
                var x = a.Value;
                var y = b.Value;
                var z = f(x, y);
                return z;
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

            base.Dispose(disposing);
        }
    }
}
