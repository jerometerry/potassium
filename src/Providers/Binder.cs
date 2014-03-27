namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// Binder performs the monadic bind operation for IProviders
    /// </summary>
    /// <typeparam name="T">Type of value stored in the source</typeparam>
    /// <typeparam name="TB">The return type of the mapping function</typeparam>
    public class Binder<T, TB> : Provider<TB>
    {
        private IProvider<T> source;
        private IProvider<Func<T, TB>> map;

        /// <summary>
        /// Constructs a new Binder from the given source and mapping function
        /// </summary>
        /// <param name="source">The source of the value to pass to the mapping function</param>
        /// <param name="map">The mapping function</param>
        public Binder(IProvider<T> source, Func<T, TB> map)
            : this(source, new Map<T, TB>(map))
        {
        }

        /// <summary>
        /// Constructs a new Binder from the given source and mapping
        /// </summary>
        /// <param name="source">The IProvider that holds the values to pass to the mapping function</param>
        /// <param name="map">The IProvider that holds the mapping functions</param>
        public Binder(IProvider<T> source, IProvider<Func<T, TB>> map)
        {
            this.source = source;
            this.map = map;
        }

        /// <summary>
        /// Performs the bind opeartions on the current value of the source and mapping providers
        /// </summary>
        public override TB Value
        {
            get
            {
                var f = this.map.Value;
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
            this.map = null;

            base.Dispose(disposing);
        }
    }
}
