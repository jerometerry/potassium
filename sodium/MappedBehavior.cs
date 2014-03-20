namespace Sodium
{
    using System;

    /// <summary>
    /// A MappedBehavior is a continuous Behavior who's value is obtained by applying
    /// a mapping function to a source Behavior.
    /// </summary>
    /// <typeparam name="T">The type of values in the source Behavior</typeparam>
    /// <typeparam name="TB">The return type of the mapping function</typeparam>
    public class MappedBehavior<T, TB> : ContinuousBehavior<TB>
    {
        private IBehavior<T> source;
        private Func<T, TB> map;

        public MappedBehavior(IBehavior<T> source, Func<T, TB> map)
        {
            this.source = source;
            this.map = map;
        }

        public override TB Value
        {
            get
            {
                return map(source.Value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.source = null;
                this.map = null;
            }

            base.Dispose(disposing);
        }
    }
}
