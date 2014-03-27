namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// UnaryLift performs the monadic bind operation for IProviders
    /// </summary>
    /// <typeparam name="T">Type of value stored in the source</typeparam>
    /// <typeparam name="TB">The return type of the mapping function</typeparam>
    public class UnaryLift<T, TB> : Provider<TB>
    {
        private IProvider<Func<T, TB>> lift;
        private IProvider<T> source;

        /// <summary>
        /// Constructs a new UnaryLift from the given source and mapping function
        /// </summary>
        /// <param name="source">The source of the value to pass to the mapping function</param>
        /// <param name="lift">The mapping function</param>
        public UnaryLift(Func<T, TB> lift, IProvider<T> source)
            : this(new Identity<Func<T, TB>>(lift), source)
        {
        }

        /// <summary>
        /// Constructs a new UnaryLift from the given source and mapping
        /// </summary>
        /// <param name="source">The IProvider that holds the values to pass to the mapping function</param>
        /// <param name="lift">The IProvider that holds the mapping functions</param>
        public UnaryLift(IProvider<Func<T, TB>> lift, IProvider<T> source)
        {
            this.source = source;
            this.lift = lift;
        }

        /// <summary>
        /// Performs the bind opeartions on the current value of the source and mapping providers
        /// </summary>
        public override TB Value
        {
            get
            {
                var f = this.lift.Value;
                var x = source.Value;
                var y = f(x);
                return y;
            }
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected override void Dispose(bool disposing)
        {
            this.source = null;
            this.lift = null;

            base.Dispose(disposing);
        }
    }
}
