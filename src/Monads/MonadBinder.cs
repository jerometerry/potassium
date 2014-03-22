namespace JT.Rx.Net.Monads
{
    using System;
    using JT.Rx.Net.Core;

    /// <summary>
    /// MonadBinder is a Monad who's value is computed by passing the current value of an IValueSource
    /// to the current mapping function of an IValueSource of functions.
    /// </summary>
    /// <typeparam name="T">Type of value stored in the source Behavior</typeparam>
    /// <typeparam name="TB">The return type of the mapping functions</typeparam>
    public class MonadBinder<T, TB> : Monad<TB>
    {
        private IProvider<T> source;
        private IProvider<Func<T, TB>> bf;

        public MonadBinder(IProvider<T> source, Func<T, TB> bf)
            : this(source, new Map<T, TB>(bf))
        {
        }

        public MonadBinder(IProvider<T> source, IProvider<Func<T, TB>> bf)
        {
            this.source = source;
            this.bf = bf;
        }

        public override TB Value
        {
            get
            {
                var map = bf.Value;
                var a = source.Value;
                var b = map(a);
                return b;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.source = null;
                this.bf = null;
            }

            base.Dispose(disposing);
        }
    }
}
