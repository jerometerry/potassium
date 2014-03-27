namespace Potassium.Providers
{
    using System;
    using Potassium.Core;

    /// <summary>
    /// A BinaryBehavior lifts a binary function into a Monad
    /// </summary>
    /// <typeparam name="T">The type of first parameter to the lift function</typeparam>
    /// <typeparam name="TB">The type of the second parameter of the lift function</typeparam>
    /// <typeparam name="TC">The return type of the lift function</typeparam>
    public class BinaryLift<T, TB, TC> : Provider<TC>
    {
        private Func<T, TB, TC> lift;
        private IProvider<T> a;
        private IProvider<TB> b;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public BinaryLift(Func<T, TB, TC> lift, IProvider<T> a, IProvider<TB> b)
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
                return lift(a.Value, b.Value);
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
