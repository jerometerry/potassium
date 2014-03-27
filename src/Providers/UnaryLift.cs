namespace Potassium.Providers
{
    using System;
    using Potassium.Core;
    using Potassium.Providers;

    /// <summary>
    /// A UnaryMonad lifts a unary function into a Monad
    /// </summary>
    /// <typeparam name="T">The type of first parameter to the lift function</typeparam>
    /// <typeparam name="TB">The return type of the lift function</typeparam>
    public class UnaryLift<T, TB> : Provider<TB>
    {
        private Func<T, TB> lift;
        private IProvider<T> source;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="source"></param>
        public UnaryLift(Func<T, TB> lift, IProvider<T> source)
        {
            this.lift = lift;
            this.source = source;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override TB Value
        {
            get
            {
                return lift(source.Value);
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
            source = null;

            base.Dispose(disposing);
        }
    }
}
