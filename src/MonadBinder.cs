namespace JT.Rx.Net
{
    
    using System;

    /// <summary>
    /// MonadBinder is a continuous Behavior who's value is computed
    /// by applying the current mapping function in a behavior of mappings functions
    /// to the current value of a behavior, producing a new Behavior that maps the 
    /// Behaviors value to the return type of the mapping function.
    /// </summary>
    /// <typeparam name="T">Type of value stored in the source Behavior</typeparam>
    /// <typeparam name="TB">The return type of the mapping functions</typeparam>
    public class MonadBinder<T, TB> : Monad<TB>
    {
        private IBehavior<T> source;
        private IBehavior<Func<T, TB>> bf;

        public MonadBinder(IBehavior<T> source, Func<T, TB> bf)
            : this(source, new Map<T, TB>(bf))
        {
        }

        public MonadBinder(IBehavior<T> source, IBehavior<Func<T, TB>> bf)
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
