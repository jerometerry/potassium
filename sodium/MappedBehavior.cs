namespace Sodium
{
    using System;

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
